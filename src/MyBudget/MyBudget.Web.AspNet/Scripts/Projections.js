
fromAll().whenAny(
       function (s, e) {
           var category = e.streamId.substring(0, e.streamId.indexOf('-'));
           if (e.streamId.indexOf("$") == -1 && category.length > 0) {
               var streamId = 'categoria_' + category;
               if (streamId.indexOf("$") == -1) {
                   linkTo(streamId, e);
               }
           }
           return null;
       });


fromStream('categoria_Lines')
   .whenAny(function (s, e) {
       var budget = e.body.BudgetId._id;
       linkTo('lines_of_' + budget, e);
       return null;
   });