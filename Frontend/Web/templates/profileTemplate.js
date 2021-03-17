var profileTemplate = {
  props: ['user', 'currentUser', 'avatar'],
  methods:
  {
    displayCount: function (n) {
      if (n >= 1e6) {
        let res = Math.round(n / 1e5) / 10;

        return res.toString() + 'M';
      }

      else if (n >= 1e3) {
        let res = Math.round(n / 1e2) / 10;

        return res.toString() + 'K';
      }

      return n > 0 ? n : null;
    }
  },

  template: `
    <div class="position-relative p-3 border-bottom bg-light">
        <input type="image" id="editAvatar"
            v-bind:src='avatar'
                class="img rounded-circle float-left" style="width: 100px; height: 100px; outline: none;"
                    alt="avatar" onclick="document.getElementById('imageInput').click()">
        <input id="imageInput" type="file" accept="image/*" enctype="multipart/form-data"
                    onchange="profile.uploadImage()" style="display: none;">

        <button class="btn btn-outline-success position-absolute m-3 top-20 end-0" data-bs-toggle="modal"
                    data-bs-target="#edit" type="button" v-if="currentUser.id == user.id">Edit profile</button>

        <p>
            <b style="font-size: large;">{{ user.name }}</b><br>
              @{{ user.username }}
        </p>
        <p style="color: black; word-wrap: break-word;">{{ user.about }}</p>
      </div>`
}