namespace Cedar.Domain.Internal
{
    using System;
    using System.Linq;
    using System.Text;
    using EnsureThat;

    public static class DeterministicEventIdGenerator
    {
        // prevents clashing with other Deterministic Guid generators. Must be different on a per library basis.
        private const string Namespace = "2CD61643-1FB7-4C2C-9871-3A3B90C0A761";

        private static readonly DeterministicGuidGenerator s_deterministicGuidGenerator
            = new DeterministicGuidGenerator(Guid.Parse(Namespace));

        public static Guid Generate(object @event, int expectedVersion, string streamId, Guid commitId)
        {
            Ensure.That(@event, "event").IsNotNull();
            Ensure.That(expectedVersion, "expectedVersion").IsGte(-2);
            Ensure.That(streamId, "streamId").IsNotNullOrWhiteSpace();
            Ensure.That(commitId, "commitId").IsNotEmpty();

            var serializedEvent = SimpleJson.SerializeObject(@event);

            var entropy = Encode(serializedEvent)
                .Concat(Encode(commitId.ToString()))
                .Concat(BitConverter.GetBytes(expectedVersion))
                .Concat(Encode(streamId));

            return s_deterministicGuidGenerator.Create(entropy);
        }

        public static Guid Generate(object @event, string streamId, int expectedVersion)
        {
            Ensure.That(@event, "event").IsNotNull();
            Ensure.That(expectedVersion, "expectedVersion").IsGte(-2);
            Ensure.That(streamId, "streamId").IsNotNullOrWhiteSpace();

            var serializedEvent = SimpleJson.SerializeObject(@event);

            var entropy = Encode(serializedEvent)
                .Concat(BitConverter.GetBytes(expectedVersion))
                .Concat(Encode(streamId));

            return s_deterministicGuidGenerator.Create(entropy);
        }

        private static byte[] Encode(string s)
        {
            return Encoding.UTF8.GetBytes(s);
        }
    }
}