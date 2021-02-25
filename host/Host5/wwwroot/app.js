var loginUrl = "/bff/login?returnUrl=/index.html?state=loggedin";
var logoutUrl = "/bff/logout?returnUrl=/index.html?state=loggedout";
var userUrl = "/bff/user";
var apiUrl = "/api";

async function onLoad() {
    var req = new Request(userUrl, { credentials: 'include' })
    var resp = await fetch(req);
    if (resp.ok) {
        log("user logged in");
        showUser(await resp.json());
    }
    else if (resp.status === 404) {
        log("user not logged in");
    }
}

onLoad();

function login() {
    window.location = loginUrl;
}
function logout() {
    window.location = logoutUrl;
}
async function callApi() {
    var req = new Request(apiUrl + "/foo", { credentials: 'include' })
    var resp = await fetch(req);
    log("API Result: " + resp.status);
    if (resp.ok) {
        showApi(await resp.json());
    }
}

document.querySelector(".login").addEventListener("click", login, false);
document.querySelector(".call").addEventListener("click", callApi, false);
document.querySelector(".logout").addEventListener("click", logout, false);


function showApi() {
    document.getElementById('api-result').innerText = '';

    Array.prototype.forEach.call(arguments, function (msg) {
        if (msg instanceof Error) {
            msg = "Error: " + msg.message;
        }
        else if (typeof msg !== 'string') {
            msg = JSON.stringify(msg, null, 2);
        }
        document.getElementById('api-result').innerText += msg + '\r\n';
    });
}

function showUser() {
    document.getElementById('user').innerText = '';

    Array.prototype.forEach.call(arguments, function (msg) {
        if (msg instanceof Error) {
            msg = "Error: " + msg.message;
        }
        else if (typeof msg !== 'string') {
            msg = JSON.stringify(msg, null, 2);
        }
        document.getElementById('user').innerText += msg + '\r\n';
    });
}

function log() {
    document.getElementById('response').innerText = '';

    Array.prototype.forEach.call(arguments, function (msg) {
        if (msg instanceof Error) {
            msg = "Error: " + msg.message;
        }
        else if (typeof msg !== 'string') {
            msg = JSON.stringify(msg, null, 2);
        }
        document.getElementById('response').innerText += msg + '\r\n';
    });
}
