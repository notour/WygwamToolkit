﻿//-----------------------------------------------------------------------
// <copyright file="NetworkChangedEventArgs.cs" company="Wygwam">
//     Copyright (c) 2013 Wygwam.
//     Licensed under the Microsoft Public License (Ms-PL) (the "License");
//     you may not use this file except in compliance with the License.
//     You may obtain a copy of the License at
//
//         http://opensource.org/licenses/Ms-PL.html
//
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wygwam.Windows.Networking
{

    public class NetworkChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:NetworkChangedEventArgs" /> class.
        /// </summary>
        /// <param name="isConnected">set to <c>true</c> if an Internet connection is available.</param>
        public NetworkChangedEventArgs(bool isConnected, InternetConnectionType connectionType = InternetConnectionType.Unknown)
        {
            this.IsConnected = isConnected;
            this.ConnectionType = connectionType;
        }

        /// <summary>
        /// Gets a value indicating whether an Internet connection is available.
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// Gets the type of the connection.
        /// </summary>
        public InternetConnectionType ConnectionType { get; private set; }
    }
}
