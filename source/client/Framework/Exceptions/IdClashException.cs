﻿namespace BlockEngine.Client.Framework.Exceptions;

public class IdClashException : Exception
{
    public IdClashException(string message) : base(message) { }
}