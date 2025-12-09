# DBD Region Changer

Dead by Daylight Region Changer is a Windows desktop tool that lets you quickly switch the matchmaking region used by the game.
The app edits the Windows hosts file to block all AWS GameLift ping endpoints and then marks exactly one selected region as “open” by commenting out its entry.

It provides a simple WinForms UI with:

    A region dropdown listing all supported AWS regions

    A live server list that shows TCP‑based latency and a quality status for each region

    A button to change the active region (with admin check and success/error feedback)

    A button to reset the hosts file and remove all GameLift overrides

The application automatically:

    Requires and relaunches itself with administrator privileges when needed

    Detects the current active region by reading the hosts file on startup and on interval

    Updates ping and status values in place without recreating the list entries

Use this project as a small utility to experiment with region locking and latency monitoring for Dead by Daylight’s GameLift infrastructure on Windows.
