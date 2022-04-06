using UnityEngine;

public class WalkingState : GroundStateUtil, IState 
{
    public IState Update(PlayerController playerController)
    {
        if (!playerController.localPlayerInputInfo.IsPressingSprint) 
            return Move(playerController.walkSpeed, playerController, this);

        return new SprintingState().Update(playerController);
    }
}