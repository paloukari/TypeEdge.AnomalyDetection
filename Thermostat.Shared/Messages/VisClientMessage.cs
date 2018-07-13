using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Amqp.Framing;

namespace ThermostatApplication.Messages
{
    public class VisClientMessage
    {
        public NewValue NewVal = new NewValue();
        public double[] FFTResult;
        public double[] SubtractionGraphResult;
        public double ReadRate;
    }
        public class NewValue
        {
            public String[] Headers;
            public double[] Inputs;
        }
}
