using System;
using System.Collections.Generic;
using System.Text;

namespace Cubes.Web.UIHelpers.Schema
{
    public interface ISchemaProvider
    {
        string Name { get; }

        Schema GetSchema();
    }
}
