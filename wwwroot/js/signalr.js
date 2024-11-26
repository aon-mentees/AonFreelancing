// signalr.js
const userId= "1";  // Replace with the actual user ID from your application
const receiverId = "2";  // Replace with the actual user ID from your application
const connection = new signalR.HubConnectionBuilder()
    .withUrl(`/chathub?userId=${userId}`)
    .configureLogging(signalR.LogLevel.Information)
    .build();

var Start = async () => {
    try {
        await connection.start();
        console.log("SignalR Connected.");
    } catch (err) {
        console.log(err);
        setTimeout(Start, 5000);
    }
};

// Start the connection.
Start();

//connection.onclose(async () => {
//    console.warn("Disconnected. Attempting to reconnect...");
//    await Start();
//});
// Sends the message to SignalR
var SendMessage = async (message) => {
    await connection.invoke("SendChatMessage", message);
}

// Sends the message when the SendMessage ID element has been clicked
document.getElementById('SendMessage').addEventListener('click', async () => {
    var message = document.getElementById('Message');

    if (message && message.value) {
        var chatMessage = {
            //sender: connection.connectionId,
            sender: userId,
            message: message.value,
            receiver: receiverId
        }
        await SendMessage(chatMessage);
        message.value = '';
    }
});
//connection.on("ReceiveMessage", (message) => {
//    var messages = document.getElementById('Messages');

//    if (messages) {
//        var bulletPoint = document.createElement('li');
//        bulletPoint.innerText = `${message.sender}: ${message.message}`; // Display sender and message
//        messages.appendChild(bulletPoint);
//    }
//});
connection.on("GetShit", (message) => console.log("got some shit"));
connection.on("ReceiveMessage", (message) => console.log(`${message.sender}: ${message.message}`));