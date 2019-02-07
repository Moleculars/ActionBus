﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Bb.ComponentModel
{

    /// <summary>
    /// Permet de retourner la liste des methodes d'evaluation disponibles dans les types fournis.
    /// </summary>
    public static class MethodDiscovery
    {

        /// <summary>
        /// Return the list of method
        /// </summary>
        /// <param name="type"></param>
        /// <param name="returnType">Not evaluated if null</param>
        /// <param name="parameters">Not evaluated if null</param>
        /// <returns></returns>
        public static IEnumerable<MethodInfo> GetMethods(Type type, BindingFlags bindings, Type returnType, List<Type> parameters)
        {
            var methods = type.GetMethods(bindings).ToList()
                .Where(c => (returnType == null || c.ReturnType == returnType) && (parameters == null || EvaluateMethodParameters(c, parameters))).ToList();
            return methods;
        }

        private static bool EvaluateMethodParameters(MethodInfo item, List<Type> parameters)
        {

            var _parameters = item.GetParameters();
            if (_parameters.Length != parameters.Count)
                return false;

            for (var i = 0; i < parameters.Count; i++)
            {
                var _p1 = _parameters[i];
                var _p2 = parameters[i];
                if (_p1.ParameterType != _p2)
                    return false;
            }

            return true;

        }


    }

}