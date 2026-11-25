using UnityEngine;
using UnityEngine.EventSystems;

public class WireTile : MonoBehaviour,
    IPointerDownHandler,
    IPointerEnterHandler,
    IPointerUpHandler
{
    public enum WireColor
    {
        None,
        Red,
        Green,
        Yellow
    }

    [Header("Tile Settings")]
    [SerializeField] private WireColor wireColor =
        WireColor.None;

    [SerializeField] private bool isTerminal;

    private WirePuzzleController puzzleController;

    public WireColor Color => wireColor;
    public bool IsTerminal => isTerminal;

    public void SetController(
        WirePuzzleController controller)
    {
        puzzleController = controller;
    }

    public void OnPointerDown(
        PointerEventData eventData)
    {
        if (puzzleController == null)
        {
            return;
        }

        puzzleController.BeginWire(this);
    }

    public void OnPointerEnter(
        PointerEventData eventData)
    {
        if (puzzleController == null)
        {
            return;
        }

        puzzleController.EnterTile(this);
    }

    public void OnPointerUp(
        PointerEventData eventData)
    {
        if (puzzleController == null)
        {
            return;
        }

        puzzleController.EndWire(this);
    }
}