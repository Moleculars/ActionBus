using Bb.Core.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Bb.Core.LocalQueue
{

    public class LQueue<T>
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        public LQueue(string path, Action<T> action, Func<T, string> serializer, Func<string, T> deserializer, Func<T, string> getKey, int split = 1000000)
        {

            this._deserializer = deserializer;
            this._serializer = serializer;
            this._count = 0;
            this._countFile = 0;
            this._action = action;
            this._split = split;
            this._toRetry = new List<BagRetry>();
            this._getKey = getKey;

            this._path = new DirectoryInfo(path);
            if (!this._path.Exists)
                this._path.Create();

            this._pathArchive = new DirectoryInfo(Path.Combine(this._path.FullName, "Archives"));
            if (!this._pathArchive.Exists)
                this._pathArchive.Create();

            Resume();

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Execute(T model)
        {

            try
            {
                this._action(model);
                return true;
            }
            catch (Exception ex)
            {
                BagRetry bag = WriteKo(model);
                this._toRetry.Add(bag);
                _count++;
            }

            return false;

        }

        /// <summary>
        /// Identify all models not precessed
        /// </summary>
        public void Resume()
        {

            _countFile = 0;
            var m1 = Path.Combine(_path.FullName, mask);

            Dictionary<string, BagRetry> _h = new Dictionary<string, BagRetry>();
            List<FileInfo> files = new List<FileInfo>();

            var files2 = new Queue<FileInfo>(_path.GetFiles().OrderBy(c => c.Name));

            // load all logs in order of the writing append
            while (files2.Count > 0)
            {
                var file = files2.Dequeue();
                using (StreamReader stream = file.OpenText())
                {

                    while (!stream.EndOfStream)
                    {

                        string line = stream.ReadLine();
                        string[] items = line.Split(';');
                        var key = items[0];
                        var state = items[1];
                        var model = items[2];

                        if (state == "ko")
                        {
                            var b = new BagRetry()
                            {
                                Key = key,
                                Message = new StringBuilder(line),
                                Model = this._deserializer(model)
                            };
                            _h.Add(key, b);
                        }
                        else
                            _h.Remove(key);

                    }

                    files.Add(file);

                }
            }

            if (_h.Count > 0)
            {

                this.filename = string.Format(m1, 0);

                // try to re execute all ko
                foreach (var item in _h)
                    Execute(item.Value);

                var pp = this._pathArchive.FullName;
                foreach (var item in files)
                    item.MoveTo(Path.Combine(pp, item.Name));

                var f = new FileInfo(this.filename);
                if (f.Exists)
                {
                    var f2 = Path.Combine(f.Directory.FullName, string.Format(m1, 1));
                    f.MoveTo(f2);
                }
            }

            _countFile = 0;
            while (File.Exists(this.filename = Path.Combine(_path.FullName, string.Format(mask, ++_countFile)))) { }

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool Execute(BagRetry bag)
        {

            try
            {
                WriteOk(bag, false);
                this._action(bag.Model);
                WriteOk(bag, true);
                return true;
            }
            catch (Exception)
            {
            }

            return false;

        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private BagRetry WriteKo(T model)
        {

            if (_count > _split)
                lock (_lock)
                    if (_count > _split)
                    {
                        _count = 0;
                        while (File.Exists(this.filename = Path.Combine(_path.FullName, string.Format(mask, ++_countFile)))) { }
                    }

            StringBuilder sb = new StringBuilder(this._serializer(model));
            var key = this._getKey(model);
            StringBuilder sb2 = new StringBuilder(key);
            sb2.Append(";ko;");
            sb2.AppendLine(sb.ToString());
            var bag = new BagRetry() { Key = key, Message = sb2, Model = model };

            lock (_lock)
                File.AppendAllText(this.filename, bag.Message.ToString());

            return bag;

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteOk(BagRetry bag, bool ok)
        {

            StringBuilder sb2 = new StringBuilder(20);

            if (ok)
            {
                sb2.Append(bag.Key);
                sb2.Append(";ok;");
            }
            else
                sb2.Append(bag.Message);

            lock (_lock)
                File.AppendAllText(this.filename, bag.Message.ToString());

        }

        private volatile object _lock = new object();
        private readonly Func<string, T> _deserializer;
        private readonly Func<T, string> _serializer;
        private int _count;
        private int _countFile;
        private string filename;
        private readonly DirectoryInfo _path;
        private readonly DirectoryInfo _pathArchive;
        private readonly Action<T> _action;
        private readonly int _split;
        private readonly List<BagRetry> _toRetry;
        private readonly Func<T, string> _getKey;
        private const string mask = "histo_{0}.txt";

        private class BagRetry
        {
            public StringBuilder Message { get; set; }
            public string Key { get; set; }
            public T Model { get; set; }
        }

    }

}
