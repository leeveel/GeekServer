/****************************************************************************
Copyright (c) Geek

https://github.com/leeveel/GeekServer

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
****************************************************************************/
using System;
using Geek.Core.Hotfix;
using System.Threading.Tasks;

namespace Geek.Core.Component
{
    public abstract class BaseComponent
    {
        internal volatile bool DidActive;
        public IComponentAgent Agent => HotfixMgr.GetAgent<IComponentAgent>(this);
        public TAgent GetAgentAs<TAgent>() where TAgent : IComponentAgent { return (TAgent)Agent; }
        public Task<TComp> GetComponent<TComp>() where TComp : BaseComponent, new() { return Actor.GetComponent<TComp>(); }
        public ComponentActor Actor { get; protected set; }
        public bool IsActive { get; protected set; }
        public long ActorId => Actor.ActorId;
        public virtual Task Active()
        {
            IsActive = true;
            return Task.CompletedTask;
        }

        public virtual Task Deactive()
        {
            return Task.CompletedTask;
        }

        internal virtual Task<bool> ReadyToDeactive()
        { 
            return Task.FromResult(true);
        }

        public virtual void Init(ComponentActor actor)
        {
            Actor = actor;
        }
    }
}