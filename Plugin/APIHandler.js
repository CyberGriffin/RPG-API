/*:
* @target MZ
* @plugindesc Enables interaction with the API through a control panel.
* @help Allows you to use the API in RPG Maker MZ.
*
* @command clear memory
* @text Clear memory
* @desc Clears the conversation memory from the current NPC.
*
* @command chat
* @text Chat
* @desc Sends a chat message to the API.
*
* @arg ConversationHistory
* @text Conversation History (Can be empty or come pre-filled to give context to the API)
* @type text
* @default
*
* @arg Character
* @text Character
* @type struct<CharacterModel>
* @default {"FirstName":"","LastName":"","Age":0,"Roles":[],"Species":"","Traits":[],"Motivations":[],"Fears":[],"Mood":"","Backstory":"","Relationships":[],"PastEvents":[],"Skills":[],"Weaknesses":[],"TaskInteractions":[],"TaskTones":[],"TaskSuccessConditions":[],"ActiveTaskId":-1,"MoralAlignment":"","PlotPoints":[],"Notes":[]}
*
*/

/*~struct~CharacterModel:
* @param FirstName
* @text First Name
* @type text
* @default
*
* @param LastName
* @text Last Name
* @type text
* @default
*
* @param Age
* @text Age
* @type number
* @default 0
*
* @param Roles
* @text Roles
* @type text[]
* @default []
*
* @param Species
* @text Species
* @type text
* @default Human
*
* @param Sex
* @text Sex
* @type text
* @default
*
* @param Traits
* @text Traits
* @type text[]
* @default []
*
* @param Motivations
* @text Motivations
* @type text[]
* @default []
*
* @param Fears
* @text Fears
* @type text[]
* @default []
*
* @param Mood
* @text Mood
* @type text
* @default
*
* @param Backstory
* @text Backstory
* @type text
* @default
*
* @param Relationships
* @text Relationships
* @type text[]
* @default []
*
* @param PastEvents
* @text Past Events
* @type text[]
* @default []
*
* @param Skills
* @text Skills
* @type text[]
* @default []
*
* @param Weaknesses
* @text Weaknesses
* @type text[]
* @default []
*
* @param TaskInteractions
* @text Task Interactions
* @type struct<TaskInteractions>[]
* @default []
*
* @param TaskTones
* @text Task Tones
* @type struct<TaskTones>[]
* @default []
*
* @param TaskSuccessConditions
* @text Task Success Conditions
* @type struct<TaskSuccessConditions>[]
* @default []
*
* @param ActiveTaskId
* @text Active Task ID
* @type number
* @default -1
*
* @param MoralAlignment
* @text Moral Alignment
* @type text
* @default
*
* @param PlotPoints
* @text Plot Points
* @type text[]
* @default []
*
* @param Notes
* @text Notes
* @type text[]
* @default []
*
*/

/*~struct~TaskInteractions:
* @param TaskId
* @text Task ID
* @type number
* @default 0
*
* @param Interaction
* @text Interaction
* @type text
* @default
*
*/

/*~struct~TaskTones:
* @param TaskId
* @text Task ID
* @type number
* @default 0
*
* @param Tone
* @text Tone
* @type text
* @default
*
*/

/*~struct~TaskSuccessConditions:
* @param TaskId
* @text Task ID
* @type number
* @default 0
*
* @param SuccessConditions
* @text Success Conditions
* @type number[]
* @default []
*
*/

(() => {

    const pluginName = "APIHandler";

    PluginManager.registerCommand(
        pluginName,
        "clear memory",
        async (args) => {
            $gameVariables.setValue(22, 0);
        }
    );

    PluginManager.registerCommand(
        pluginName,
        "chat",
        async (args) => {

            // Set variables
            $gameVariables.setValue(21, 0);
            $gameVariables.setValue(23, 0);
            $gameVariables.setValue(24, 0);
            $gameVariables.setValue(25, 0);
            $gameVariables.setValue(26, 0);
            $gameVariables.setValue(27, 0);

            // Get user input
            let message = window.prompt("Enter message");
            if (message === null) return;
            
            let parsedCharacter = parseCharacterModel(args.Character);
            var userMessage = new UserMessage(
                message,
                $gameVariables.value(22) || args.ConversationHistory || "",
                JSON.stringify(parsedCharacter)
            );

            const response = await sendChatMessage(
                userMessage
            );

            $gameVariables.setValue(21, chunkMessage(response.message));
            $gameVariables.setValue(22, response.conversationHistory);
            $gameVariables.setValue(23, response.IsTaskComplete);
            $gameVariables.setValue(24, response.ratings[0]);
            $gameVariables.setValue(25, response.ratings[1]);
            $gameVariables.setValue(26, response.ratings[2]);
            $gameVariables.setValue(27, response.ratings[3]);
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
    const response = await fetch('https://rpgaiservice.azurewebsites.net/Character', {
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

class UserMessage {
    constructor(
        message,
        conversationHistory,
        characterJson
    ) {
        this.Message = message;
        this.ConversationHistory = conversationHistory;
        this.CharacterJson = characterJson;
    }
}

function parseCharacterModel (json) {
        let data = JSON.parse(json);

        data.Age = parseInt(data.Age);
        data.Roles = JSON.parse(data.Roles);

        data.Traits = JSON.parse(data.Traits);
        data.Motivations = JSON.parse(data.Motivations);
        data.Fears = JSON.parse(data.Fears);

        data.Relationships = JSON.parse(data.Relationships);
        data.PastEvents = JSON.parse(data.PastEvents);

        data.Skills = JSON.parse(data.Skills);
        data.Weaknesses = JSON.parse(data.Weaknesses);

        let TaskInteractions = {};
        let TaskTones = {};
        let TaskSuccessConditions = {};

        let key = null;
        let value = null;
        Object.entries(JSON.parse(JSON.parse(data.TaskInteractions))).forEach(([_, v]) => {
            if (key === null) {
                key = parseInt(v);
            } else {
                value = v;
            }

            if (key !== null && value !== null) {
                TaskInteractions[key] = value;
                key = null;
                value = null;
            }
        });

        data.TaskInteractions = TaskInteractions;

        key = null;
        value = null;
        Object.entries(JSON.parse(JSON.parse(data.TaskTones))).forEach(([_, v]) => {
            if (key === null) {
                key = parseInt(v);
            } else {
                value = v;
            }

            if (key !== null && value !== null) {
                TaskTones[key] = value;
                key = null;
                value = null;
            }
        });

        data.TaskTones = TaskTones;

        key = null;
        value = null;
        Object.entries(JSON.parse(JSON.parse(data.TaskSuccessConditions))).forEach(([_, v]) => {
            if (key === null) {
                key = parseInt(v);
            } else {
                value = JSON.parse(v).map(x => parseInt(x));
            }

            if (key !== null && value !== null) {
                TaskSuccessConditions[key] = value;
                key = null;
                value = null;
            }
        });

        data.TaskSuccessConditions = TaskSuccessConditions;

        data.ActiveTaskId = parseInt(data.ActiveTaskId);
        data.PlotPoints = JSON.parse(data.PlotPoints);

        data.Notes = JSON.parse(data.Notes);

        return data;
    }
