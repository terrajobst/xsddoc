using System;

namespace XsdDocumentation.Model
{
    internal sealed class Context
    {
        public Context(IMessageReporter messageReporter, Configuration configuration)
        {
            ProblemReporter = new ProblemReporter(messageReporter);
            Configuration = configuration;

            SchemaSetManager = new SchemaSetManager(this);
            SchemaSetManager.Initialize();

            SourceCodeManager = new SourceCodeManager(this);
            SourceCodeManager.Initialize();

            TopicManager = new TopicManager(this);
            TopicManager.Initialize();

            DocumentationManager = new DocumentationManager(this);
            DocumentationManager.Initialize();
        }

        public ProblemReporter ProblemReporter { get; private set; }
        public Configuration Configuration { get; private set; }
        public SchemaSetManager SchemaSetManager { get; private set; }
        public SourceCodeManager SourceCodeManager { get; private set; }
        public TopicManager TopicManager { get; private set; }
        public DocumentationManager DocumentationManager { get; private set; }
    }
}