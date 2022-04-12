using System;

namespace QBProcessor
{
    /// <summary>RequestEventReplySender Class</summary>
    public class RequestEventReplySender
    {
        /// <summary>Gets or sets the requester identifier.</summary>
        /// <value>The requester identifier.</value>
        public Guid RequesterId { get; set; }

        /// <summary>Gets or sets the type of the request.</summary>
        /// <value>The type of the request.</value>
        public string RequestType { get; set; }

        /// <summary>Gets or sets the qbxml request.</summary>
        /// <value>The qbxml request.</value>
        public string QbRequest { get; set; }
        
        /// <summary>Gets or sets the qbxml response.</summary>
        /// <value>The qbxml response.</value>
        public string QbResponse { get; set; }
        
        /// <summary>Gets or sets the sender.</summary>
        /// <value>The sender.</value>
        public object Sender { get; set; }

        /// <summary>Initializes a new instance of the <see cref="RequestEventReplySender" /> class.</summary>
        public RequestEventReplySender() 
        { 
            RequesterId = Guid.NewGuid(); 
        }

        /// <summary>Initializes a new instance of the <see cref="RequestEventReplySender" /> class.</summary>
        /// <param name="sender">The sender.</param>
        public RequestEventReplySender(object sender) 
        { 
            Sender = sender; 
            RequesterId = Guid.NewGuid(); 
        }

        /// <summary>Initializes a new instance of the <see cref="RequestEventReplySender" /> class.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="requestType">Type of the request.</param>
        /// <param name="response">The response.</param>
        public RequestEventReplySender(object sender, Guid guid, string requestType, string response) 
        { 
            Sender = sender; 
            RequesterId = guid; 
            RequestType = requestType;
            QbResponse = response; 
        }

        /// <summary>Initializes a new instance of the <see cref="RequestEventReplySender" /> class.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="requestType">Type of the request.</param>
        /// <param name="qbRequest">The qb request.</param>
        /// <param name="response">The response.</param>
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
