using System;

namespace RestClient.Exceptions
{
    public class ControlNotFoundException : Exception
    {
        public override string Message => "Keyboard.Control can't be a null";
    }
}