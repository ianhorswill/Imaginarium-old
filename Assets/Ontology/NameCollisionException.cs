using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class NameCollisionException : Exception
{
    public NameCollisionException(string[] name, Type oldType, Type newType)
        : base($"Can't create a new {newType}, {name.Untokenize()}, because there is already {oldType} of the same name.")
    { }
}
