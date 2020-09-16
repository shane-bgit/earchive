using System;
using System.Collections.Generic;
using System.Text;

namespace DMS.Core.Communication.Bucket
{
    public class CreateBucketResponse
    {
        /// <summary>
        ///  ID that uniquely identifies a request. Amazon keeps track of request IDs. 
        ///  If you have a question about a request, include the request ID in your corresopndence.
        /// </summary>
        public string RequestId { get; set; }

        /// <summary>
        /// Name of the Bucket created.
        /// </summary>
        public string BucketName { get; set; }
    }
}
