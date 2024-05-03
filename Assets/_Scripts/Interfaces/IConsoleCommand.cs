using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Console
{
  public interface IConsoleCommand
  {
    string Command { get; }
    bool Process(string[] args);
    string WrongInputMessage { get; }
    string SuccessMessage { get; }
  }
}
