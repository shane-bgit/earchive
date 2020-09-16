using System;
using System.Collections.Generic;
using System.Text;

namespace DMS.Core.Communication.Bucket
{
    public class ListS3BucketResponse
    {
        public string BucketName { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
