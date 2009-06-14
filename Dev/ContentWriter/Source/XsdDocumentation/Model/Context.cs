using System;

namespace XsdDocumentation.Model
{
	internal sealed class Context
	{
		private ProblemReporter _problemReporter;
		private Configuration _configuration;
		private SchemaSetManager _schemaSetManager;
		private SourceCodeManager _sourceCodeManager;
		private TopicManager _topicManager;
		private DocumentationManager _documentationManager;

		public Context(IMessageReporter messageReporter, Configuration configuration)
		{
			_problemReporter = new ProblemReporter(messageReporter);
			_configuration = configuration;
			_schemaSetManager = new SchemaSetManager(this);
			_sourceCodeManager = new SourceCodeManager(this);
			_topicManager = new TopicManager(this);
			_documentationManager = new DocumentationManager(this);

			_schemaSetManager.Initialize();
			_sourceCodeManager.Initialize();
			_topicManager.Initialize();
			_documentationManager.Initialize();
		}

		public ProblemReporter ProblemReporter
		{
			get { return _problemReporter; }
		}

		public Configuration Configuration
		{
			get { return _configuration; }
		}

		public SchemaSetManager SchemaSetManager
		{
			get { return _schemaSetManager; }
		}

		public SourceCodeManager SourceCodeManager
		{
			get { return _sourceCodeManager; }
		}

		public TopicManager TopicManager
		{
			get { return _topicManager; }
		}

		public DocumentationManager DocumentationManager
		{
			get { return _documentationManager; }
		}
	}
}
