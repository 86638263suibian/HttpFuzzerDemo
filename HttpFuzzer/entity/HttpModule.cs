﻿using System;
using System.Collections.Generic;
using System.Text;

namespace LoveCoody.entity
{
    public class HttpModule
    {
        private String method;

        public String Method
        {
            get { return method; }
            set { method = value; }
        }
        private String encode;

        public String Encode
        {
            get { return encode; }
            set { encode = value; }
        }
      
        private Dictionary<String, String> header;

        public Dictionary<String, String> Header
        {
            get { return header; }
            set { header = value; }
        }

        private String body;

        public String Body
        {
            get { return body; }
            set { body = value; }
        }

        private Boolean isHex = false;

        public Boolean IsHex
        {
            get { return isHex; }
            set { isHex = value; }
        }
    }
}
