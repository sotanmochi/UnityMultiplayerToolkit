//
// References:
//
//   - Creating Servers for Multiplayer Mobile Games with Just a Few Lines of JavaScript - AWS Game Tech Blog
//     - https://aws.amazon.com/blogs/gametech/creating-servers-for-multiplayer-mobile-games-with-amazon-gamelift/
//     - https://github.com/aws-samples/megafrograce-gamelift-realtime-servers-sample
//
//   - BatteryAcid's video
//     - https://youtu.be/WaAZyqgkXDY
//     - https://gist.github.com/BatteryAcid/a8d687f460a5b90fc403c600d1541707
//

const AWS = require('aws-sdk');

exports.handler = async (event) => {
    
    let response;

    let GameLift = new AWS.GameLift({region: process.env.AWS_REGION});
    let fleetId = process.env['FleetId'];
    let playerSessionMaxCount = process.env['MaximumPlayerSessionCount'];

    let requestedGameSessionName = event.RoomName;
    let requestedPlayerSessionId = event.PlayerId;

    // find any sessions that have available players
    let gameSessions;
    await GameLift.searchGameSessions({
        FleetId: fleetId,
        FilterExpression: "hasAvailablePlayerSessions=true"
    }).promise().then(data => {
        gameSessions = data.GameSessions;
    }).catch(err => {
        response = err;
    });

    // if the response object has any value at any point before the end of
    // the function that indicates a failure condition so return the response
    if (response != null) 
    {
        return response;
    }

    let selectedGameSession;
    gameSessions.forEach((gameSession) => 
    {
        if (gameSession.Name === requestedGameSessionName)
        {
            selectedGameSession = gameSession;
        }
    });

    if (selectedGameSession == null)
    {
        await GameLift.createGameSession({
            MaximumPlayerSessionCount: playerSessionMaxCount,
            Name: requestedGameSessionName,
            FleetId: fleetId
        }).promise().then(data => {
            console.log("Created game session: ", data.GameSession.Name);
            selectedGameSession = data.GameSession;
        }).catch(err => {
           response = err; 
        });
    }

    // if the response object has any value at any point before the end of
    // the function that indicates a failure condition so return the response
    if (response != null) 
    {
        return response;
    }

    let playerSession;
    if (selectedGameSession != null) 
    {
        await GameLift.createPlayerSession({
            GameSessionId : selectedGameSession.GameSessionId,
            PlayerId: requestedPlayerSessionId
        }).promise().then(data => {
            console.log("Created player session ID: ", data.PlayerSession.PlayerSessionId);
            playerSession = data.PlayerSession;
        }).catch(err => {
           response = err; 
        });
    }

    // if the response object has any value at any point before the end of
    // the function that indicates a failure condition so return the response
    if (response != null) 
    {
        return response;
    }

    response = {
        IpAddress: selectedGameSession.IpAddress,
        Port: selectedGameSession.Port,
        RoomName: selectedGameSession.Name,
    };

    return response;
};