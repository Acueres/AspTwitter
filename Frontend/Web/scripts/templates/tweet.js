var tweet = {
  props: ['entry', 'index', 'avatar', 'user', 'deleteEntry'],
  methods:
  {
    displayLikes: function (n) {
      if (n >= 1e6) {
        let res = Math.round(n / 1e5) / 10;

        return res.toString() + 'M';
      }

      else if (n >= 1e3) {
        let res = Math.round(n / 1e2) / 10;

        return res.toString() + 'K';
      }

      return n == 0 ? null : n;
    },

    liked: function (user, id) {
      return user.favorites.includes(id);
    },

    addLike: function (user, entry) {
      user.favorites.push(entry.id);
      entry.likesCount++;
      fetch(`http://localhost:5000/api/entries/${entry.id}/favorite`, {
        method: 'POST',
        credentials: 'omit',
        redirect: 'follow',
        cache: 'no-cache',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer ' + user.token
        }
      });
    },

    removeLike: function (user, entry) {
      let index = user.favorites.indexOf(entry.id);
      user.favorites.splice(index, 1);
      entry.likesCount--;
      fetch(`http://localhost:5000/api/entries/${entry.id}/favorite`, {
        method: 'DELETE',
        credentials: 'omit',
        redirect: 'follow',
        cache: 'no-cache',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer ' + user.token
        }
      });
    }
  },
  template: `
    <div class="row border-bottom p-3">
      <div class="col-auto">
        <img v-bind:src='avatar' class="img rounded-circle"
          style="width: 50px; height: 50px; outline: none;" alt="avatar">
      </div>

      <div class="col-auto">
        <b style="font-size: large;">{{ entry.author.name }}</b> @{{ entry.author.username }}

        <button type="button" class="btn btn-default" aria-label="Left Align"
          v-on:click='deleteEntry(index, entry.id)' v-if="entry.author.id == user.id">
          <span class="la la-trash" aria-hidden="true"></span>
        </button> <br>
        <a style="color: black;">{{ entry.text }}</a> <br>

        <div class="btn-group" role="group" aria-label="Basic example">
          <button type="button" class="btn btn-default shadow-none" aria-label="Left Align" v-if="user.logged">
            <span class="la la-comment-dots" aria-hidden="true"></span>
          </button>

          <button type="button" class="btn btn-default shadow-none" aria-label="Left Align" v-if="user.logged">
            <span class="la la-retweet" aria-hidden="true"></span>
          </button>

          <button type="button" class="btn btn-default shadow-none" aria-label="Left Align" v-if="user.logged">
            <span class="la la-heart" aria-hidden="true" v-bind:style="liked(user, entry.id) ? 'color: blue;': ''"
             v-on:click="liked(user, entry.id) ? removeLike(user, entry): addLike(user, entry)"></span>
            <span>{{ displayLikes(entry.likesCount) }}</span>
          </button>
        </div>

      </div>
    </div>`
}