$.getJSON('http://revamp.ap-southeast-1.elasticbeanstalk.com/catalogue', function(data) {
    console.log(data);
    // var rawData = '[{"item1":"tag1","a1":"b1"},{"item2":"tag2","a2":"b2"}]';
    var parsed = JSON.parse(data);
    console.log(parsed[0].a1); // logs "b1"
    console.log(parsed[1].a2); // logs "b2"
});

function firstbuild() {
    $.getJSON('http://revamp.ap-southeast-1.elasticbeanstalk.com/catalogue', function(data) {
        console.log(data);
});
}
