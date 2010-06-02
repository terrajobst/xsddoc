using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace XsdDocumentation.Model
{
    public sealed class TopicIndex
    {
        private List<TopicIndexEntry> _entries = new List<TopicIndexEntry>();
        private Dictionary<string, TopicIndexEntry> _entryDictionary = new Dictionary<string, TopicIndexEntry>();

        private void Clear()
        {
            _entries.Clear();
            _entryDictionary.Clear();
        }

        internal void Load(TopicManager topicManager)
        {
            Clear();
            AddEntries(topicManager.Topics);
        }

        private void AddEntries(IEnumerable<Topic> topics)
        {
            foreach (var topic in topics)
            {
                if (topic.LinkUri != null)
                    AddEntry(topic.Id, topic.LinkTitle, topic.LinkUri);
                if (topic.LinkIdUri != null)
                    AddEntry(topic.Id, topic.LinkTitle, topic.LinkIdUri);

                AddEntries(topic.Children);
            }
        }

        private void AddEntry(string topicId, string linkTitle, string uri)
        {
            if (_entryDictionary.ContainsKey(uri))
                return;

            var entry = new TopicIndexEntry
                        {
                            TopicId = topicId,
                            LinkTitle = linkTitle,
                            Uri = uri
                        };
            _entries.Add(entry);
            _entryDictionary.Add(uri, entry);
        }

        public void Load(string fileName)
        {
            Clear();

            var doc = new XmlDocument();
            doc.Load(fileName);

            var entryNodes = doc.SelectNodes("//entry");
            if (entryNodes == null)
                return;

            foreach (XmlElement entryNode in entryNodes)
            {
                var topicId = entryNode.Attributes["topicId"].Value;
                var linkTitle = entryNode.Attributes["linkTitle"].Value;
                var uri = entryNode.Attributes["uri"].Value;
                AddEntry(topicId, linkTitle, uri);
            }
        }

        public void Save(string fileName)
        {
            var doc = new XmlDocument();
            var root = doc.CreateElement("topicIndex");
            doc.AppendChild(root);

            foreach (var entry in _entries)
            {
                var entryNode = doc.CreateElement("entry");
                entryNode.SetAttribute("topicId", entry.TopicId);
                entryNode.SetAttribute("linkTitle", entry.LinkTitle);
                entryNode.SetAttribute("uri", entry.Uri);
                root.AppendChild(entryNode);
            }

            var directory = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            doc.Save(fileName);
        }

        public TopicIndexEntry FindEntry(string uri)
        {
            TopicIndexEntry result;
            _entryDictionary.TryGetValue(uri, out result);
            return result;
        }
    }
}