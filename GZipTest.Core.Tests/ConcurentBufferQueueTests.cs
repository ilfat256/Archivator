using Microsoft.VisualStudio.TestTools.UnitTesting;
using GZipTest.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZipTest.Core.Tests
{
    [TestClass()]
    public class ConcurentBufferQueueTests
    {    
        [TestMethod()]
        public void Enqueue_WhenAdded1Item_CountShouldBe1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void Enqueue_WhenAdded10Item_CountShouldBe10()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void Dequeue_WhenEmptyQueue_ShouldWait()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void Dequeue_When1ThreadEnqueu10Elements_2ThreadShouldDequeu10()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void Dequeue_When2ThreadEnqueu10Elements_1ThreadShouldDequeu10()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void Dequeue_When2ThreadEnqueu10Elements_2ThreadShouldDequeu10()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void Dequeue_WhenEmpty_GettingShouldWait()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void Enqueue_WhenFull_AddingShouldWait()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void Enqueue_WhenWorks_CountShouldntBeMoreBufferSize()
        {
            Assert.Fail();
        }
    }
}