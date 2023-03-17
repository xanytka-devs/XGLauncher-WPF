using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XGL.Common {

    public enum InternalError {

        API_CRASHED,
        API_STARTUP_FAILURE,
        API_NULL,
        API_NOT_FOUND,
        API_NOT_SUPPORTED,
        API_NETWORK_ERROR,
        API_SERVER_ERROR,
        API_UNKNOWN_ERROR,

    }

    public enum ExternalError {

        NET_NOCONNECTION,
        NET_NETWORK_ERROR,
        NET_SERVER_ERROR,
        NET_FAST_CLOCK,

    }

    public class ErrorAnalyzer {



    }

}
