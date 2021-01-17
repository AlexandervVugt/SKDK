
function displayConvo(chatId) {
    document.getElementById("active_chat").value = chatId;
    let old = document.querySelector(".conversation_entry.active");
    if (old !== null) {
        old.classList.remove("active");
    }
    document.querySelector(".conversation_entry#c" + chatId).classList.add("active");
    document.getElementById("chattitle").innerHTML = ChatInbox[chatId].Listing.Title;
    let chatDiv = document.getElementById("messagelist");
    chatDiv.innerHTML = "";
    ChatInbox[chatId].Messages.forEach(function(message){
        let div = document.createElement("div");
        div.classList.add('message_container');
        let info = document.createElement("span");
        info.classList.add("message_info");
        if (message.SenderId === userId) {
            //display on right side
            div.classList.add("message_right_align");
        } else {
            //display on left side
            div.classList.add("message_left_align");
        }
        /*info.innerHTML = message.SenderId === ChatInbox[chatId].Responder.Id ?
            ChatInbox[chatId].Responder.Username : ChatInbox[chatId].Poster.Username;*/
        info.innerHTML = message.Timestamp.split('T')[1].split('+')[0];
        div.appendChild(info);
        let content = document.createElement("p");
        content.classList.add("message_content");
        content.innerHTML = message.Content;
        div.appendChild(content);
        chatDiv.appendChild(div);
    });
    chatDiv.scrollTop = chatDiv.scrollHeight;
}

function hideForm() {
    let errorElement = document.getElementById("errorpopup");
    errorElement.style.display = "none";
    
}

function setBlockInputs() {
    let activechat = document.querySelector(".conversation_entry.active").id.substring(1);
    console.log("active set to: " + activechat);
    document.querySelector("#confirmBlock > input[name='chatId']").value = activechat;
}

function confirmToBlock() {
    let confirmMessage = document.getElementById("confirmBlock");
    confirmMessage.style.display = "block";
}


function closeBlock(form) {
    let blockForm = document.getElementById(form);
    blockForm.style.display = "none";
}
