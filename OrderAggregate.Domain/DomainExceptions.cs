using System;
using System.Collections.Generic;
using System.Text;

namespace OrderAggregate.Domain;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

public class InvalidOrderStateException : DomainException
{
    public InvalidOrderStateException(string message) : base(message) { }
}