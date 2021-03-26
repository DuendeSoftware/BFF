var loginUrl = "/bff/login";
var logoutUrl = "/bff/logout";
var userUrl = "/bff/user";
var localApiUrl = "/local";
var remoteApiUrl = "/api";

async function onLoad() {
    var req = new Request(userUrl, {
        headers: new Headers({
            'X-CSRF': '1'
        })
    })

    try {
        var resp = await fetch(req);
        if (resp.ok) {
            log("user logged in");
            showUser(await resp.json());
        } else if (resp.status === 401) {
            log("user not logged in");
        }
    }
    catch (e) {
        log("error checking user status");
    }
}

onLoad();

function login() {
    window.location = loginUrl;
}

function logout() {
    window.location = logoutUrl;
}

async function callLocalApi() {
    var req = new Request(localApiUrl, {
        headers: new Headers({
            'X-CSRF': '1'
        })
    })
    var resp = await fetch(req);

    log("API Result: " + resp.status);
    if (resp.ok) {
        showApi(await resp.json());
    }
}

async function callCrossApi() {
    var req = new Request(remoteApiUrl + "/foo", {
        headers: new Headers({
            'X-CSRF': '1'
        })
    })
    var resp = await fetch(req);

    log("API Result: " + resp.status);
    if (resp.ok) {
        showApi(await resp.json());
    }
}


document.querySelector(".login").addEventListener("click", login, false);
document.querySelector(".call_cross").addEventListener("click", callCrossApi, false);
document.querySelector(".call_local").addEventListener("click", callLocalApi, false);
document.querySelector(".logout").addEventListener("click", logout, false);


function showApi() {
    document.getElementById('api-result').innerText = '';

    Array.prototype.forEach.call(arguments, function (msg) {
        if (msg instanceof Error) {
            msg = "Error: " + msg.message;
        } else if (typeof msg !== 'string') {
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
        } else if (typeof msg !== 'string') {
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
        } else if (typeof msg !== 'string') {
            msg = JSON.stringify(msg, null, 2);
        }
        document.getElementById('response').innerText += msg + '\r\n';
    });
}
