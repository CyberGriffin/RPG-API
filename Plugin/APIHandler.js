/*:
* @target MZ
* @plugindesc Enables interaction with the API through a control panel.
* @help Allows you to use the API in RPG Maker MZ.
*
* @command chat
* @text Chat
* @desc Sends a chat message to the API.
*
* @arg Task
* @text Task Success Condition. (What does the player need to do to succeed?)
* @type text
* @default
*
* @arg ConversationTopics
* @text Conversation Topics. (condition): response
* @type text[]
* @default []
*/

(() => {

    const pluginName = "APIHandler";

    PluginManager.registerCommand(
        pluginName,
        "chat",
        async (args) => {

            // Set variables
            $gameVariables.setValue(21, 0);
            $gameVariables.setValue(22, 0);
            $gameVariables.setValue(23, 0);
            $gameVariables.setValue(24, 0);
            $gameVariables.setValue(25, 0);
            $gameVariables.setValue(26, 0);
            $gameVariables.setValue(27, 0);

            // Get user input
            let message = window.prompt("Enter message");
            if (message === null) return;

            const response = await sendChatMessage({
                "Task": args.Task,
                "ConversationTopics": [args.ConversationTopics],
                "PlayerPrompt": message
            });
            
            console.log(response.intent);
            if (response.intent === "Task")
            { // Set variables
                $gameVariables.setValue(21, chunkMessage(response.taskModel.feedback));
                $gameVariables.setValue(22, response.isSuccess);
                $gameVariables.setValue(23, response.isCrit);
                $gameVariables.setValue(24, response.tone);
                $gameVariables.setValue(26, response.intent);
            }
            else if (response.intent === "ConversationTopics")
            { // Set variables
                $gameVariables.setValue(25, chunkMessage(response.conversationTopicModel.response));
            }
            else {
                console.log(response);
            }

            $gameVariables.setValue(27, 1);
        }
    );
})();

document.onkeydown = function (e) {
    if (e.keyCode == 27) {
        $gameVariables.setValue(16, 0);
    }
}

function chunkMessage(message) {
    let msgChucks = message.match(/(.{1,45}\b)/g);
    let combinedChunks = msgChucks.join('\n');
    return combinedChunks;
}

async function sendChatMessage(message) {
    const response = await fetch('https://rpgaiservice.azurewebsites.net/Orchestrator/RequestOrchestrator', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(message)
    });

    const text = await response.text();
    try {
        return JSON.parse(text);
    } catch (error) {
        console.error('Error parsing response from API', error);
    }
}

