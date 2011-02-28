﻿using System;
using OGDotNet.Mappedtypes.engine.View.permission;
using OGDotNet.Mappedtypes.LiveData;
using OGDotNet.Model.Resources;
using OGDotNet.Tests.Integration.Xunit.Extensions;
using Xunit;
using FactAttribute = OGDotNet.Tests.Integration.Xunit.Extensions.FactAttribute;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class RemoteViewProcessorTests : ViewTestsBase
    {
        [Fact]
        public void CanGet()
        {
            var remoteViewProcessor = Context.ViewProcessor;
            Assert.NotNull(remoteViewProcessor);
        }

        [Fact]
        public void CanGetNames()
        {
            var remoteViewProcessor = Context.ViewProcessor;
            var viewNames = remoteViewProcessor.ViewNames;
            Assert.NotEmpty(viewNames);
            Assert.DoesNotContain(null, viewNames);
            Assert.DoesNotContain("", viewNames);
        }

        [Theory]
        [TypedPropertyData("ViewNames")]
        public void CanGetViews(string viewName)
        {
            var remoteViewResource = Context.ViewProcessor.GetView(viewName);
            Assert.NotNull(remoteViewResource);
            Assert.Equal(viewName, remoteViewResource.Name);

            
        }

        [Theory]
        [TypedPropertyData("Views")]
        public void CanGetViewsDefinitions(RemoteView remoteView)
        {
            var viewDefinition = remoteView.Definition;
            Assert.NotNull(viewDefinition);
        }

        [Theory]
        [TypedPropertyData("Views")]
        public void CanGetViewsPortfoliosAfterInit(RemoteView remoteView)//NOTE: I can't test the reverse, since the view might have been inited elsewhere
        {
            remoteView.Init();
            var portfolio = remoteView.Portfolio;

            if (remoteView.Name != "Primitives Only")
            {
                Assert.NotNull(portfolio);
            }
            else
            {
                Assert.Null(portfolio);
            }
        }
        [Theory]
        [TypedPropertyData("Views")]
        public void CanAssertAccessAfterInit(RemoteView remoteView)
        {
            remoteView.Init();

            remoteView.AssertAccessToLiveDataRequirements(new UserPrincipal("bbgintegrationtestuser", "127.0.0.1"));
        }

        [Theory]
        [TypedPropertyData("Views")]
        public void CanAssertFailedAccessAfterInit(RemoteView remoteView)
        {
            remoteView.Init();

            var userPrincipal = new UserPrincipal("someOtherUser"+Guid.NewGuid(), "127.0.0.1");
            var viewPermissionException = Assert.Throws<ViewPermissionException>(() => remoteView.AssertAccessToLiveDataRequirements(userPrincipal));
            Assert.Contains(userPrincipal.UserName, viewPermissionException.Message);
        }
    }
}
