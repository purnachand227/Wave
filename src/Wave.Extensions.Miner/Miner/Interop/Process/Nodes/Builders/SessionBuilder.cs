﻿using System.Collections.Generic;

namespace Miner.Interop.Process
{
    /// <summary>
    ///     A list builder that can be used to build session nodes.
    /// </summary>
    public class SessionBuilder : BasePxNodeBuilder
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="SessionBuilder" /> class.
        /// </summary>
        /// <param name="pxApp">The process application refernece.</param>
        /// <param name="nodeIDs">The node IDs.</param>
        public SessionBuilder(IMMPxApplication pxApp, IList<int> nodeIDs)
            : base("Session Builder", pxApp, nodeIDs, Session.NodeTypeName)
        {
        }

        #endregion

        #region Protected Methods

        /// <summary>
        ///     Gets the node with the specified <paramref name="nodeID" />.
        /// </summary>
        /// <param name="nodeID">The node ID.</param>
        /// <returns>
        ///     Returns the <see cref="Miner.Interop.Process.IMMPxNode" /> representing the node.
        /// </returns>
        protected override IMMPxNode GetNode(int nodeID)
        {
            var o = new Session(base.PxApplication, nodeID);
            return o.Valid ? o.Node : null;
        }

        #endregion
    }
}