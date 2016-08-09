using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace opcode4.utilities
{
    public class ConsoleArguments : IEnumerable
    {
        private readonly StringDictionary _parameters;

        public ConsoleArguments(IEnumerable<string> args)
        {
            _parameters = new StringDictionary();
            var spliter = new Regex(@"^-{1,2}|^/|=",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

            var remover = new Regex(@"^['""]?(.*?)['""]?$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

            string parameter = null;

            foreach (var txt in args)
            {
                var parts = spliter.Split(txt, 3);

                switch (parts.Length)
                {
                    case 1:
                        if (parameter != null)
                        {
                            if (!_parameters.ContainsKey(parameter))
                            {
                                parts[0] =
                                    remover.Replace(parts[0], "$1");

                                _parameters.Add(parameter, parts[0]);
                            }
                            parameter = null;
                        }
                        else
                            throw new Exception("no parameter waiting for a value");

                        break;

                    case 2:
                        if (parameter != null)
                        {
                            if (!_parameters.ContainsKey(parameter))
                                _parameters.Add(parameter, "");
                        }
                        parameter = parts[1];
                        break;

                    case 3:
                        if (parameter != null)
                        {
                            if (!_parameters.ContainsKey(parameter))
                                _parameters.Add(parameter, "");
                        }

                        parameter = parts[1];

                        // Remove possible enclosing characters (",')
                        if (!_parameters.ContainsKey(parameter))
                        {
                            parts[2] = remover.Replace(parts[2], "$1");
                            _parameters.Add(parameter, parts[2]);
                        }

                        parameter = null;
                        break;
                }
            }
            // In case a parameter is still waiting
            if (parameter != null)
            {
                if (!_parameters.ContainsKey(parameter))
                    _parameters.Add(parameter, "");
            }
        }

        public string this[string param]
        {
            get
            {
                return _parameters[param];
            }
        }

        public IEnumerator GetEnumerator()
        {
            return _parameters.Keys.GetEnumerator();
        }
    }
}