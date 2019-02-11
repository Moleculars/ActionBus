namespace Bb.RabbitMq.LogAppender
{

    /// <summary>
    /// Service to configure
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IConfiguring<T> where T : IConfigurator<T>
    {

        void Configure(IConfigurator<T> configuration);

    }

        /// <summary>
    /// Class use to configure a service
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IConfigurator
    {

        bool AcceptConfigure(object objectToConfigure);

        void Configure(object objectToConfigure);

    }


    /// <summary>
    /// Class use to configure a service
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IConfigurator<T> : IConfigurator
    {

    }

}
