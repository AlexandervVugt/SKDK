
function displayConvo(chatId) {
    document.getElementById("active_chat").value = chatId;
    document.getElementById("chattitle").innerHTML = ChatInbox[chatId].Listing.Title;
    let chatDiv = document.getElementById("messagelist");
    chatDiv.innerHTML = "";
    ChatInbox[chatId].Messages.forEach(function(message){
        let span = document.createElement("span");
        span.classList.add('message_container');
        let info = document.createElement("span");
        info.classList.add("message_info");
        info.innerHTML = message.SenderId === ChatInbox[chatId].Responder.Id ?
            ChatInbox[chatId].Responder.Username : ChatInbox[chatId].Poster.Username;
        info.innerHTML += " om " + message.Timestamp;
        span.appendChild(info);
        let content = document.createElement("p");
        content.classList.add("message_content");
        content.innerHTML = message.Content;
        span.appendChild(content);
        chatDiv.appendChild(span);
    });
}