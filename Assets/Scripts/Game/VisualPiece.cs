using System.Collections.Generic;
using UnityChess;
using UnityEngine;
using static UnityChess.SquareUtil;
using Unity.Netcode;


/// <summary>
/// Represents a visual chess piece in the game. This component handles user interaction,
/// such as dragging and dropping pieces, and determines the closest square on the board
/// where the piece should land. It also raises an event when a piece has been moved.
/// </summary>
public class VisualPiece : MonoBehaviour {
	// Delegate for handling the event when a visual piece has been moved.
	// Parameters: the initial square of the piece, its transform, the closest square's transform,
	// and an optional promotion piece.
	public delegate void VisualPieceMovedAction(Square movedPieceInitialSquare, Transform movedPieceTransform, Transform closestBoardSquareTransform, Piece promotionPiece = null);
	
	// Static event raised when a visual piece is moved.
	public static event VisualPieceMovedAction VisualPieceMoved;
	
	// The colour (side) of the piece (White or Black).
	public Side PieceColor;
	
	// Retrieves the current board square of the piece by converting its parent's name into a Square.
	public Square CurrentSquare => StringToSquare(transform.parent.name);
	
	// The radius used to detect nearby board squares for collision detection.
	private const float SquareCollisionRadius = 9f;
	
	// The camera used to view the board.
	private Camera boardCamera;
	// The screen-space position of the piece when it is first picked up.
	private Vector3 piecePositionSS;
	// A reference to the piece's SphereCollider (if required for collision handling).
	private SphereCollider pieceBoundingSphere;
	// A list to hold potential board square GameObjects that the piece might land on.
	private List<GameObject> potentialLandingSquares;
	// A cached reference to the transform of this piece.
	private Transform thisTransform;

	/// <summary>
	/// Initialises the visual piece. Sets up necessary variables and obtains a reference to the main camera.
	/// </summary>
	private void Start() {
		// Initialise the list to hold potential landing squares.
		potentialLandingSquares = new List<GameObject>();
		// Cache the transform of this GameObject for efficiency.
		thisTransform = transform;
		// Obtain the main camera from the scene.
		boardCamera = Camera.main;
	}

	/// <summary>
	/// Called when the user presses the mouse button over the piece.
	/// Records the initial screen-space position of the piece.
	/// </summary>
	public void OnMouseDown()
	{
    	if (!enabled) return;

    	// ✅ Multiplayer turn-lock check
    	if (NetworkPlayer.LocalPlayerInstance == null)
    	{
        	Debug.Log("No local player instance.");
        	return;
    	}

    	// ✅ Check if piece belongs to the player
    	if (NetworkPlayer.LocalPlayerInstance.PlayerSide != PieceColor)
    	{
        	Debug.Log("This piece does not belong to you!");
        	return;
    	}

    	// ✅ Check if it's your turn
    	if (!NetworkPlayer.LocalPlayerInstance.IsMyTurn())
    	{
        	Debug.Log("Not your turn!");
        	return;
    	}

    	// Continue with normal interaction
    	piecePositionSS = boardCamera.WorldToScreenPoint(transform.position);
	}


	/// <summary>
	/// Called while the user drags the piece with the mouse.
	/// Updates the piece's world position to follow the mouse cursor.
	/// </summary>
	private void OnMouseDrag() {
		if (enabled) {
			// Create a new screen-space position based on the current mouse position,
			// preserving the original depth (z-coordinate).
			Vector3 nextPiecePositionSS = new Vector3(Input.mousePosition.x, Input.mousePosition.y, piecePositionSS.z);
			// Convert the screen-space position back to world-space and update the piece's position.
			thisTransform.position = boardCamera.ScreenToWorldPoint(nextPiecePositionSS);
		}
	}

	/// <summary>
	/// Called when the user releases the mouse button after dragging the piece.
	/// Determines the closest board square to the piece and raises an event with the move.
	/// </summary>
	public void OnMouseUp()
	{
    	if (!enabled) return;

    	potentialLandingSquares.Clear();
    	BoardManager.Instance.GetSquareGOsWithinRadius(potentialLandingSquares, thisTransform.position, SquareCollisionRadius);

    	if (potentialLandingSquares.Count == 0)
    	{
        	thisTransform.position = thisTransform.parent.position;
        	return;
    	}

    	Transform closestSquareTransform = potentialLandingSquares[0].transform;
    	float shortestDistanceFromPieceSquared = (closestSquareTransform.position - thisTransform.position).sqrMagnitude;

    	for (int i = 1; i < potentialLandingSquares.Count; i++)
    	{
        	GameObject potentialLandingSquare = potentialLandingSquares[i];
        	float distanceFromPieceSquared = (potentialLandingSquare.transform.position - thisTransform.position).sqrMagnitude;

        	if (distanceFromPieceSquared < shortestDistanceFromPieceSquared)
        	{
            	shortestDistanceFromPieceSquared = distanceFromPieceSquared;
            	closestSquareTransform = potentialLandingSquare.transform;
        	}
    	}

    	// 🛑 Multiplayer checks
    	if (NetworkManager.Singleton.IsConnectedClient)
    	{
        	if (NetworkPlayer.LocalPlayerInstance == null)
        	{
            	Debug.Log("No local player instance.");
            	thisTransform.position = thisTransform.parent.position;
            	return;
        	}

        	if (NetworkPlayer.LocalPlayerInstance.PlayerSide != PieceColor)
        	{
            	Debug.Log("This piece doesn't belong to you!");
            	thisTransform.position = thisTransform.parent.position;
            	return;
        	}

        	if (!NetworkPlayer.LocalPlayerInstance.IsMyTurn())
        	{
            	Debug.Log("Not your turn!");
            	thisTransform.position = thisTransform.parent.position;
            	return;
        	}

        	// ✅ Send move request to host/server
        	GameManager.Instance.RequestMoveServerRpc(
            	CurrentSquare.ToString(),
            	closestSquareTransform.name,
            	NetworkManager.Singleton.LocalClientId
        	);

        	// Reset position until move is approved and synced
        	thisTransform.position = thisTransform.parent.position;
        	return;
    	}

    	// 👇 Optional: fallback for offline/local mode
    	VisualPieceMoved?.Invoke(CurrentSquare, thisTransform, closestSquareTransform);
	}
}
