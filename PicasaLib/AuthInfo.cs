using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PicasaLib
{
    class AuthInfo
    {


        private String _accessToken;

        public String AccessToken
        {
            get { return _accessToken; }
        }

        private String _refreshToken;

        public String RefreshToken
        {
            get { return _refreshToken; }
        }

        private DateTime _timeOut;

        public DateTime TimeOut
        {
            get { return _timeOut; }
        }

        private bool _success;

        public bool Success
        {
            get { return _success; }
        }

		public AuthInfo() {

			_success = false;
		}

		public AuthInfo(String accessToken, String refreshToken, int timeOut) {

			_accessToken = accessToken;
			_refreshToken = refreshToken;

			_timeOut = DateTime.Now.AddSeconds(timeOut);

			_success = true;
		}

		bool isActive() {

			if(DateTime.Now > TimeOut) return(false);
			else return(true);
		}
    }
}
