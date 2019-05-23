using System;
using System.Collections.Generic;

namespace ExitGames.Client.Photon
{
  internal class InvocationCache
  {
    private readonly LinkedList<InvocationCache.CachedOperation> cache = new LinkedList<InvocationCache.CachedOperation>();
    private int nextInvocationId = 1;

    public int NextInvocationId
    {
      get
      {
        return this.nextInvocationId;
      }
    }

    public int Count
    {
      get
      {
        return this.cache.Count;
      }
    }

    public void Reset()
    {
      lock (this.cache)
      {
        this.nextInvocationId = 1;
        this.cache.Clear();
      }
    }

    public void Invoke(int invocationId, Action action)
    {
      lock (this.cache)
      {
        if (invocationId < this.nextInvocationId)
          return;
        if (invocationId == this.nextInvocationId)
        {
          ++this.nextInvocationId;
          action();
          if (this.cache.Count <= 0)
            return;
          LinkedListNode<InvocationCache.CachedOperation> linkedListNode = this.cache.First;
          while (linkedListNode != null && linkedListNode.Value.InvocationId == this.nextInvocationId)
          {
            ++this.nextInvocationId;
            linkedListNode.Value.Action();
            linkedListNode = linkedListNode.Next;
            this.cache.RemoveFirst();
          }
        }
        else
        {
          InvocationCache.CachedOperation cachedOperation = new InvocationCache.CachedOperation()
          {
            InvocationId = invocationId,
            Action = action
          };
          if (this.cache.Count == 0)
          {
            this.cache.AddLast(cachedOperation);
          }
          else
          {
            for (LinkedListNode<InvocationCache.CachedOperation> node = this.cache.First; node != null; node = node.Next)
            {
              if (node.Value.InvocationId > invocationId)
              {
                this.cache.AddBefore(node, cachedOperation);
                return;
              }
            }
            this.cache.AddLast(cachedOperation);
          }
        }
      }
    }

    private class CachedOperation
    {
      public int InvocationId { get; set; }

      public Action Action { get; set; }
    }
  }
}
