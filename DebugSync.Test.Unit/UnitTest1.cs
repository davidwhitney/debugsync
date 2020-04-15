using System.Net.Http;
using NUnit.Framework;

namespace DebugSync.Test.Unit
{
    public class SessionTests
    {
        private Session _sut;

        [SetUp]
        public void Setup()
        {
            _sut = new Session();
        }

        [Test]
        public void Process_MessageIsNewConnection_HoldsClient()
        {
            var req = new ProtocolRequest {ClientId = "client1", Message = "join" };
            var response = _sut.Process(req);
            
            Assert.That(response.Action, Is.EqualTo(Action.Hold));
        }

        [Test]
        public void Process_MessageIsStepButSessionNotStarted_HoldsClient()
        {
            var response = _sut.Process(new ProtocolRequest {ClientId = "client1", Message = "step" });
            
            Assert.That(response.Action, Is.EqualTo(Action.Hold));
        }

        [Test]
        public void Process_StartedAndMessageIsStepAndCounterIsCurrent_ReturnsContinue()
        {
            _sut.Process(new ProtocolRequest { ClientId = "client1", Message = "join"  });

            _sut.Started = true;
            var response = _sut.Process(new ProtocolRequest {ClientId = "client1", Message = "step" });
            
            Assert.That(response.Action, Is.EqualTo(Action.Continue));
        }

        [Test]
        public void Process_StepRequestAndAllClientsOnZero_ReturnsContinue()
        {
            _sut.Process(new ProtocolRequest {ClientId = "client1", Message = "join" });
            _sut.Process(new ProtocolRequest {ClientId = "client2", Message = "join" });
            _sut.Started = true;

            var response = _sut.Process(new ProtocolRequest { ClientId = "client1", Message = "step"  });

            Assert.That(response.Action, Is.EqualTo(Action.Continue));
        }

        [Test]
        public void Process_AutostartIsTrueNewClient_ReturnsContinueIfInSync()
        {
            _sut.Started = true;

            var response = _sut.Process(new ProtocolRequest { ClientId = "client1", Message = "step"  });

            Assert.That(response.Action, Is.EqualTo(Action.Continue));
        }

        [Test]
        public void Process_AllClientsArentInSync_ReturnsHold()
        {
            _sut.Started = true;

            _sut.Process(new ProtocolRequest { ClientId = "client1", Message = "step"  });
            _sut.Process(new ProtocolRequest { ClientId = "client1", Message = "step"  });
            var c2response = _sut.Process(new ProtocolRequest { ClientId = "client2", Message = "step"  });

            Assert.That(c2response.Action, Is.EqualTo(Action.Continue));
        }

        [Test]
        public void Process_AllClientsArentInSync_LetsClientCatchup()
        {
            _sut.Started = true;
            _sut.HoldUntil = 0;

            _sut.Process(new ProtocolRequest { ClientId = "client1", Message = "step"  });
            _sut.Process(new ProtocolRequest { ClientId = "client1", Message = "step"  });
            var c2response = _sut.Process(new ProtocolRequest { ClientId = "client2", Message = "step"  });
            var c1response = _sut.Process(new ProtocolRequest { ClientId = "client1", Message = "step"  });

            Assert.That(c1response.Action, Is.EqualTo(Action.Hold));
        }

        [Test]
        public void Process_ClientsInSync_ReturnsContinue()
        {
            _sut.Started = true;

            _sut.Process(new ProtocolRequest { ClientId = "client1", Message = "step"  });
            _sut.Process(new ProtocolRequest { ClientId = "client2", Message = "step"  });
            var response = _sut.Process(new ProtocolRequest { ClientId = "client1", Message = "step"  });

            Assert.That(response.Action, Is.EqualTo(Action.Continue));
        }

        [Test]
        public void Process_HoldUntilNotMet_ReturnsHold()
        {
            _sut.HoldUntil = 2;
            _sut.Started = true;

            _sut.Process(new ProtocolRequest { ClientId = "client1", Message = "blah" });
            var response = _sut.Process(new ProtocolRequest { ClientId = "client1", Message = "blah2" });
            
            Assert.That(response.Action, Is.EqualTo(Action.Hold));
        }

        [Test]
        public void Process_ClientsInSyncMessageDoesntMatch_ReturnsBreak()
        {
            _sut.Started = true;

            _sut.Process(new ProtocolRequest { ClientId = "client1", Message = "step"  });
            _sut.Process(new ProtocolRequest { ClientId = "client2", Message = "step"  });
            _sut.Process(new ProtocolRequest { ClientId = "client1", Message = "step"  });
            _sut.Process(new ProtocolRequest { ClientId = "client2", Message = "step"  });

            var response1 = _sut.Process(new ProtocolRequest { ClientId = "client1", Message = "foo"  });
            var response2 = _sut.Process(new ProtocolRequest { ClientId = "client2", Message = "bar"  });
            var response3 = _sut.Process(new ProtocolRequest { ClientId = "client1", Message = "bar"  });

            Assert.That(response1.Action, Is.EqualTo(Action.Continue));
            Assert.That(response2.Action, Is.EqualTo(Action.Break));
            Assert.That(response3.Action, Is.EqualTo(Action.Break));
        }
    }
}