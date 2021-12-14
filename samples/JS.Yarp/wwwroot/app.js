const loginUrl = "/bff/login";
const userUrl = "/bff/user";
let logoutUrl = "/bff/logout";

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

            let claims = await resp.json();
            showUser(claims);

            let logoutUrlClaim = claims.find(claim => claim.type === 'bff:logout_url');
            if (logoutUrlClaim) {
                logoutUrl = logoutUrlClaim.value;
            }
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

async function callUserToken() {
    var req = new Request("/user_api", {
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

async function callClientToken() {
    var req = new Request("/client_api", {
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

async function callNoToken() {
    var req = new Request("/anon_api", {
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
document.querySelector(".logout").addEventListener("click", logout, false);

document.querySelector(".call_user").addEventListener("click", callUserToken, false);
document.querySelector(".call_client").addEventListener("click", callClientToken, false);
document.querySelector(".call_anon").addEventListener("click", callNoToken, false);



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
