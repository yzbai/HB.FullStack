using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HB.Infrastructure.RabbitMQ
{
    public class DynamicTaskCollection
    {
        private object _taskNodesLocker = new object();
        private LinkedList<TaskNode> _taskNodes = new LinkedList<TaskNode>();

        private Action _taskProcedure;
        private ulong _perThreadFacingCount;
        private int _maxTaskNumber;

        public DynamicTaskCollection(Action taskProcedure, int initialTaskNumber, int maxTaskNumber, ulong perThreadFacingCount)
        {
            if (initialTaskNumber > maxTaskNumber)
            {
                throw new ArgumentException($"DynamicTaskCollection 的 初始任务数量不能比最大任务数量还要大.");
            }

            _taskProcedure = taskProcedure;
            _maxTaskNumber = maxTaskNumber;
            _perThreadFacingCount = perThreadFacingCount;

            CoordinateTaskNumber(initialTaskNumber);
        }

        public void CoordinateTaskNumber(int newInitialTaskNumber)
        {
            lock (_taskNodesLocker)
            {
                //first clean finished tasks
                LinkedListNode<TaskNode> node = _taskNodes.First;

                while (node != null)
                {
                    LinkedListNode<TaskNode> nextNode = node.Next;

                    if (node.Value.Task.IsCompleted)
                    {
                        _taskNodes.Remove(node);
                    }

                    node = nextNode;
                }

                //caculate task number
                if (newInitialTaskNumber > _taskNodes.Count)
                {
                    int toAddNumber = newInitialTaskNumber - _taskNodes.Count;

                    if (toAddNumber <= _maxTaskNumber)
                    {
                        AddTaskAndStart(toAddNumber);
                    }
                }
            }
        }

        private void AddTaskAndStart(int taskCount)
        {
            lock (_taskNodesLocker)
            {
                if (taskCount > _maxTaskNumber)
                {
                    return;
                }

                for (int i = 0; i < taskCount; ++i)
                {
                    CancellationTokenSource cs = new CancellationTokenSource();

                    Task task = Task.Factory.StartNew(_taskProcedure, cs.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

                    TaskNode taskNode = new TaskNode() { Task = task, CancellationTokenSource = cs };

                    _taskNodes.AddLast(taskNode);
                }
            }
        }
    }
}
