using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QBProcessor
{
    public class RequestEventReplySender
    {
        public Guid RequesterId { get; set; }
        public string RequestType { get; set; }
        public string QbRequest { get; set; }
        public string QbResponse { get; set; }
        public object Sender { get; set; }

        public RequestEventReplySender() 
        { 
            RequesterId = Guid.NewGuid(); 
        }
        public RequestEventReplySender(object sender) 
        { 
            Sender = sender; 
            RequesterId = Guid.NewGuid(); 
        }
        public RequestEventReplySender(object sender, Guid guid, string requestType, string response) 
        { 
            Sender = sender; 
            RequesterId = guid; 
            RequestType = requestType;
            QbResponse = response; 
        }
        public RequestEventReplySender(object sender, Guid guid, string requestType, string qbRequest, string response) 
        { 
            Sender = sender; 
            RequesterId = guid; 
            RequestType = requestType; 
            QbRequest = qbRequest;
            QbResponse = response; 
        }
    }
}
