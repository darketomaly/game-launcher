using System;
using System.Collections.Generic;
using System.Text;

namespace game_launcher
{
    struct Version
    {

        internal static Version zero = new Version(0, 0, 0);

        private short major;
        private short minor;
        private short subMinor;

        internal Version(short _major, short _minor, short _subMinor)
        {

            major = _major;
            minor = _minor;
            subMinor = _subMinor;
        }

        internal Version(string _version)
        {

            string[] _versionStrings = _version.Split('.');

            if (_versionStrings.Length != 3)
            {

                major = 0;
                minor = 0;
                subMinor = 0;
                return;
            }

            major = short.Parse(_versionStrings[0]);
            minor = short.Parse(_versionStrings[1]);
            subMinor = short.Parse(_versionStrings[2]);
        }

        internal bool IsDifferentThan(Version _otherVersion)
        {

            if (major != _otherVersion.major)
            {

                return true;

            }
            else
            {

                if (minor != _otherVersion.minor)
                {
                    return true;

                }
                else
                {

                    if (subMinor != _otherVersion.subMinor)
                        return true;
                }
            }

            return false;
        }

        public override string ToString()
        {

            return $"{major}.{minor}.{subMinor}";
        }
    }
}
