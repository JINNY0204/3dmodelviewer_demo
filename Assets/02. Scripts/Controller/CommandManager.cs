using System.Collections.Generic;
using UnityEngine;
public interface ICommand
{
    void Execute();
    void Undo();
}

public class CommandManager : MonoBehaviour
{
    private static Stack<ICommand> commandStack = new Stack<ICommand>();
    private static Stack<ICommand> redoStack = new Stack<ICommand>();
    public float undoLimit;

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Z))
        {
            Undo();
        }
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Y))
        {
            Redo();
        }
    }

    public static void ExecuteCommand(ICommand command)
    {
        command.Execute();
        commandStack.Push(command);
        redoStack.Clear();
    }

    public static void Undo()
    {
        if (commandStack.Count > 0)
        {
            ProcessManager.Instance.UserSystemMessage("�������");

            ICommand command = commandStack.Pop();
            command.Undo();
            redoStack.Push(command);
        }
    }

    public static void Redo()
    {
        if (redoStack.Count > 0)
        {
            ProcessManager.Instance.UserSystemMessage("�ٽý���");

            ICommand command = redoStack.Pop();
            command.Execute();
            commandStack.Push(command); // �ٽ� ������ Command�� ���� ���ÿ� �߰�
        }
    }

    public static void ClearAllStack()
    {
        commandStack.Clear();
        redoStack.Clear();
    }
}

#region Ŀ��Ʈ
public class ColorChangeCommand : ICommand
{
    GameObject target;
    Material[] originalMaterials;
    Material[] newMaterials;

    public ColorChangeCommand(GameObject target, Material[] originalMaterials, Material[] newMaterials)
    {
        this.target = target;
        this.originalMaterials = originalMaterials;
        this.newMaterials = newMaterials;
    }

    public void Execute() 
    { 
        Renderer renderer = target.GetComponent<Renderer>();
        renderer.materials = newMaterials;
    }

    public void Undo() 
    {
        Renderer renderer = target.GetComponent<Renderer>();
        renderer .materials = originalMaterials;
    }
}

public class TransparencyChangeCommand : ICommand
{
    GameObject target;
    Material[] originalMaterials;
    Material[] newMaterials;

    public TransparencyChangeCommand(GameObject target, Material[] originalMaterials, Material[] newMaterials)
    {
        this.target = target;
        this.originalMaterials = originalMaterials;
        this.newMaterials = newMaterials;
    }

    public void Execute()
    {
        Renderer _rend = target.GetComponent<Renderer>();
        _rend.materials = newMaterials;
    }

    public void Undo()
    {
        Renderer _rend = target.GetComponent<Renderer>();
        _rend.materials = originalMaterials;
    }
}

public class EraseCommand : ICommand
{
    GameObject target;
    bool state;

    public EraseCommand(GameObject target, bool state)
    {
        this.target = target;
        this.state = state;
    }

    public void Execute()
    {
        target.SetActive(state);
    }

    public void Undo()
    {
        target.SetActive(!state);
    }
}
#endregion