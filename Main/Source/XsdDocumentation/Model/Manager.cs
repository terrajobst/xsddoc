using System;

namespace XsdDocumentation.Model
{
	internal abstract class Manager
	{
		protected Manager(Context context)
		{
			Context = context;
		}

		public virtual void Initialize()
		{
		}

		public Context Context { get; private set; }
	}
}