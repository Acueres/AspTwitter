function uploadAvatar() {
    let avatar = document.getElementById("avatar-selector");

    let reader = new FileReader();

    let mb = 1024 * 1024;

    if (avatar.files && avatar.files[0] && avatar.files[0].size <= mb) {
        reader.onload = (e) => {
            document.getElementById("edit-avatar").src = e.target.result;
        }
        reader.readAsDataURL(avatar.files[0]);

        let image = avatar.files[0];
        let form = new FormData();
        form.append("avatar", image);

        let settings = {
            "async": true,
            "crossDomain": true,
            "url": server + `api/users/${appUser.id}/avatar`,
            "method": "POST",
            "processData": false,
            "contentType": false,
            "mimeType": "multipart/form-data",
            "data": form,
            beforeSend: function (xhr) {
                xhr.setRequestHeader("Authorization", 'Bearer ' + appUser.token);
                xhr.setRequestHeader('ApiKey', apiKey);
            }
        };

        jQuery.ajax(settings);
    }
}

document.onkeydown = function (e) {
    if (e.key == 'Enter') {
        e.preventDefault();
    }
};


$(function () {
    $("#nav").load("components/nav.html", function () {
        $.getScript("scripts/nav.js");
    });
});

$(function () {
    $("#home").load("components/home.html", function () {
        $.getScript("scripts/home.js");
    });
});

$(function () {
    $("#explore").load("components/explore.html", function () {
        $.getScript("scripts/explore.js");
    });
});

$(function () {
    $("#profile").load("components/profile/profile.html", function () {
        $.getScript("scripts/profile/profile.js");
    });
});

$(function () {
    $("#edit-profile").load("components/profile/edit-profile.html", function () {
        $.getScript("scripts/profile/edit-profile.js");
    });
});

$(function () {
    $("#delete-profile").load("components/profile/delete-profile.html", function () {
        $.getScript("scripts/profile/delete-profile.js");
    });
});

$(function () {
    $("#login").load("components/auth/login.html", function () {
        $.getScript("scripts/auth/login.js");
    });
});

$(function () {
    $("#register").load("components/auth/register.html", function () {
        $.getScript("scripts/auth/register.js");
    });
});

$(function () {
    $("#tweet").load("components/tweet/tweet.html", function () {
        $.getScript("scripts/tweet/tweet.js");
    });
});

$(function () {
    $("#edit-tweet").load("components/tweet/edit-tweet.html", function () {
        $.getScript("scripts/tweet/edit-tweet.js");
    });
});