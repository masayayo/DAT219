var basePath = "http://tpage.azurewebsites.net/images";

$(document).on("change", "#fileToUpload", (function(){
    previewPicture(this);
}));

function previewPicture(input) {
    
	if (input.files && input.files[0]) {
        var reader = new FileReader();

        reader.onload = function (e) {
            $("div.tournament-picture").css({"background": "url(\""+e.target.result+"\")", "background-size": "cover", "background-position": "center"});
        }

        reader.readAsDataURL(input.files[0]);
    }
}

$(window).load(function(){
    console.log("load");
    // Note: new Date().getTime() is used as a cachebreaker at the end so that the 
    // pictures are updated correctly. Without it, the browser would show the old
    // cached profile picture (if there was one).
    $("div.tournament-picture").css({"background": "url(\""+basePath+"/featured-default.jpg"+"?" + new Date().getTime()+"\")", "background-size": "cover", "background-position": "center"});

});