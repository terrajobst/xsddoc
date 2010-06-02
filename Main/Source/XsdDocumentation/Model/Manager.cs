using System;

namespace XsdDocumentation.Model
{
    internal abstract class Manager
    {
        protected Manager(Context context)
        {
            Context = context;
        }

        public abstract void Initialize();

        protected Context Context { get; private set; }
    }
}