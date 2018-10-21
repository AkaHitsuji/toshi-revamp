# import logging

from flask import request, jsonify

from revamp import application as app
from revamp import db

# logger = logging.getLogger(__name__)


@app.route('/user/<user_id>', methods=['GET'])
def get_user_data(user_id):
    result = db.get(user_id)
    return jsonify(result)


@app.route('/user/<user_id>/shopping', methods=['GET'])
def get_shopping_list(user_id):
    result = db.get(user_id)['shopping']
    return jsonify(result)


@app.route('/user/<user_id>/shopping/add/list/<list_id>', methods=['GET'])
def add_list_to_shopping(user_id, list_id):
    result = db.get(user_id)

    if list_id not in result['lists']:
        return jsonify({"error": "list not found"})
    list_to_add = result['lists'][list_id]['items']
    for i in range(len(list_to_add)):
        list_to_add[i]['status'] = 'searching'

    result['shopping']['items'] += list_to_add
    db.update(user_id, result)

    return jsonify(result['shopping'])


@app.route('/user/<user_id>/shopping/add/item/vumark/<vumark_id>', methods=['GET'])
def add_item_to_cart(user_id, vumark_id):
    result = db.get(user_id)

    if vumark_id not in vumark:
        return jsonify({"error": "invalid vumark"})
    item_id = vumark_to_id(vumark_id)
    if not item_id:
        return jsonify({"error": "invalid item"})

    for i in range(len(result['shopping']['items'])):
        if result['shopping']['items'][i]['id'] == item_id:
            result['shopping']['items'][i]['status'] = 'in_cart'

            result['shopping']['subtotal'] = calculate_shopping_price(
                result['shopping']['items'])
            result['shopping']['total'] = calculate_shopping_price(
                result['shopping']['items'])
            db.update(user_id, result)
            return jsonify(result['shopping'])

    item = mapping[item_id]
    item['status'] = 'in_cart'
    item['qty'] = 1
    result['shopping']['items'].append(item)
    result['shopping']['subtotal'] = calculate_shopping_price(
        result['shopping']['items'])
    result['shopping']['total'] = calculate_shopping_price(
        result['shopping']['items'])
    db.update(user_id, result)

    return jsonify(result['shopping'])

@app.route('/user/<user_id>/shopping/add/item/normal/<item_id>', methods=['GET'])
def add_normal_item_to_cart(user_id, item_id):
    result = db.get(user_id)

    item_id = int(item_id)
    if item_id not in mapping:
        return jsonify({"error": "invalid item"})

    for i in range(len(result['shopping']['items'])):
        if result['shopping']['items'][i]['id'] == item_id:
            result['shopping']['items'][i]['status'] = 'in_cart'

            result['shopping']['subtotal'] = calculate_shopping_price(
                result['shopping']['items'])
            result['shopping']['total'] = calculate_shopping_price(
                result['shopping']['items'])
            db.update(user_id, result)
            return jsonify(result['shopping'])

    item = mapping[item_id]
    item['status'] = 'in_cart'
    item['qty'] = 1
    result['shopping']['items'].append(item)
    result['shopping']['subtotal'] = calculate_shopping_price(
        result['shopping']['items'])
    result['shopping']['total'] = calculate_shopping_price(
        result['shopping']['items'])
    db.update(user_id, result)

    return jsonify(result['shopping'])

@app.route('/user/<user_id>/list', methods=['GET'])
def get_user_all_list(user_id):
    result = db.get(user_id)['lists']
    return jsonify(result)


@app.route('/user/<user_id>/list/<list_id>', methods=['GET'])
def get_user_list(user_id, list_id):
    data = db.get(user_id)
    result = data['lists']
    if list_id in result:
        result = result[list_id]
    else:
        data['lists'][list_id] = {
            "items": [],
            "subtotal": 0,
            "discount": 0,
            "total": 0
        }
        db.update(user_id, data)
        result = {
            "items": []
        }
    return jsonify(result)


def calculate_price(items):
    price_cents = 0
    for item in items:
        item_cost_cents = int(round(item['price'] * 10)*10)
        price_cents += item['qty']*item_cost_cents
    return price_cents/100


def calculate_shopping_price(items):
    price_cents = 0
    for item in items:
        if item['status'] == 'in_cart':
            item_cost_cents = int(round(item['price'] * 10)*10)
            price_cents += item['qty']*item_cost_cents
    return price_cents/100


@app.route('/user/<user_id>/list/<list_id>/add/<item_id>', methods=['GET'])
def add_item_to_list(user_id, list_id, item_id):
    data = db.get(user_id)
    result = data['lists']
    if list_id in result:
        if int(item_id) in mapping:
            item = mapping[int(item_id)]
        else:
            return jsonify({"error": "item not found"})
        added = False
        for i in range(len(data['lists'][list_id]['items'])):
            if data['lists'][list_id]['items'][i]['id'] == int(item_id):
                data['lists'][list_id]['items'][i]['qty'] += 1
                added = True
                break
        if not added:
            item['qty'] = 1
            data['lists'][list_id]['items'].append(item)

        data['lists'][list_id]['subtotal'] = calculate_price(
            data['lists'][list_id]['items'])
        data['lists'][list_id]['total'] = calculate_price(
            data['lists'][list_id]['items'])
        db.update(user_id, data)
        result = {"status": "item added"}
    else:
        return jsonify({"error": "list not found"})

    return jsonify(result)


@app.route('/user/<user_id>/list/<list_id>/delete/<item_id>', methods=['GET'])
def remove_item_from_list(user_id, list_id, item_id):
    data = db.get(user_id)
    result = data['lists']
    if list_id in result:
        if int(item_id) in mapping:
            # item = mapping[int(item_id)]
            pass
        else:
            return jsonify({"error": "item not found"})
        added = False
        for i in range(len(data['lists'][list_id]['items'])):
            if data['lists'][list_id]['items'][i]['id'] == int(item_id):
                if data['lists'][list_id]['items'][i]['qty'] == 1:
                    del data['lists'][list_id]['items'][i]
                else:
                    data['lists'][list_id]['items'][i]['qty'] -= 1
                added = True
                break
        if not added:
            return jsonify({"error": "item not in list"})
        data['lists'][list_id]['subtotal'] = calculate_price(
            data['lists'][list_id]['items'])
        data['lists'][list_id]['total'] = calculate_price(
            data['lists'][list_id]['items'])
        db.update(user_id, data)
        result = {"status": "item removed"}
    else:
        return jsonify({"error": "list not found"})

    return jsonify(result)


@app.route('/catalogue', methods=['GET'])
def get_catalogue():
    return jsonify(all_item)


@app.route('/catalogue/<category>', methods=['GET'])
def get_catalogue_by_cat(category):
    category = category.lower()
    if category == 'meats':
        category = 'meat'
    elif category == 'vegetables':
        category = 'vegetable'
    elif category == 'fruits':
        category = 'fruit'

    if category in catalogue:
        result = catalogue[category]
    else:
        result = []
    return jsonify(result)
    # result = {"items": catalogue[category]}
    # return jsonify(result)


# @app.route('/hardcode', methods=['GET'])
# def hardcode():
#     # data = request.get_json()
#     # app.logger.info("data sent for evaluation {}".format(data))
#     # inputValue = data.get("input")
#     # result = inputValue * inputValue
#     # app.logger.info("My result :{}".format(result))

#     response = {
#         "items": [
#             {
#                 "id": 1,
#                 "name": "jelly",
#                 "qty": 2,
#                 "price": 9.54
#             }, {
#                 "id": 2,
#                 "name": "peanut",
#                 "qty": 1,
#                 "price": 15.24
#             }, {
#                 "id": 3,
#                 "name": "shampoo",
#                 "qty": 3,
#                 "price": 1.00
#             }, {
#                 "id": 4,
#                 "name": "spam",
#                 "qty": 1,
#                 "price": 0.25
#             }
#         ]
#     }
#     return jsonify(response)
#     # return jsonify(result)


# @app.route('/hardcode2', methods=['GET'])
# def hardcode2():
#     response = [
#         {
#             "id": 1,
#             "name": "jelly",
#             "qty": 2,
#             "price": 9.54
#         }, {
#             "id": 2,
#             "name": "peanut",
#             "qty": 1,
#             "price": 15.24
#         }, {
#             "id": 3,
#             "name": "shampoo",
#             "qty": 3,
#             "price": 1.00
#         }, {
#             "id": 4,
#             "name": "spam",
#             "qty": 1,
#             "price": 0.25
#         }
#     ]

#     return jsonify(response)


@app.route('/item/<item_id>', methods=['GET'])
def get_item(item_id):
    if item_id not in vumark:
        return jsonify({"error": "vumark not registered"})
    response = mapping[vumark[item_id]]
    if item_id == '0004':
        response['golden'] = True
    else:
        response['golden'] = False
    return jsonify(response)


@app.route('/normal/item/<item_id>', methods=['GET'])
def get_item_by_item_id(item_id):
    item_id = int(item_id)
    if item_id not in mapping:
        return jsonify({"error": "invalid item"})
    response = mapping[item_id]
    response['golden'] = False
    return jsonify(response)

@app.route('/user/<user_id>/reset', methods=['GET'])
def reset_user(user_id):
    db.get(user_id)
    data = db.empty_json_data()
    db.update(user_id,data)
    return jsonify(data)


vumark = {
    '0001': 31,
    '0002': 32,
    '0003': 34,
    '0004': 31
}


def vumark_to_id(vumark_id):
    if vumark_id in vumark:
        return vumark[vumark_id]
    else:
        return 0


def id_to_vumark(id):
    for key in vumark:
        if vumark[key] == id:
            return key
    return 0


meat = [
    {
        "id": 11,
        "name": "Chicken - Kampong",
        "weight": "900g",
        "price": 7.40,
        "country": "Malaysia",
        "description": "Fresh chicken parts. Freshness guaranteed. Estimated product life for 3 days including delivery day.",
        "url": "https://s3-ap-southeast-1.amazonaws.com/www8.fairprice.com.sg/fpol/media/images/product/XL/13097670_XL1.jpg",
        "rating": "4.0/5 (451 ratings)",
        "serial": "54651598453",
        "supplier": "Kee Song"
    },
    {
        "id": 12,
        "name": "Australia Pork - Minced",
        "weight": "300g",
        "price": 4.10,
        "country": "Australia",
        "description": "Fresh pork direct flown from Australia.",
        "url": "https://s3-ap-southeast-1.amazonaws.com/www8.fairprice.com.sg/fpol/media/images/product/XL/13132878_XL1.jpg",
        "rating": "4.2/5 (451 ratings)",
        "serial": "54651598453",
        "supplier": "NTUC"
    },
    {
        "id": 13,
        "name": "Tochigi Wagyu A4 Steak",
        "weight": "200g",
        "price": 57.70,
        "country": "Japan",
        "description": "Steak sliced from A4 grade wagyu. Our Tochigi Wagyu is on par with many famous Japanese beef brands. Deriving from pure lineage, these black haired cattles are raised only by designated commercial farmers passing strict requirements. Smooth and marbled meat with rich flavor.",
        "url": "https://s3-ap-southeast-1.amazonaws.com/media.redmart.com/newmedia/1600x/i/m/2000005002007_0094_1455666654471.jpg",
        "rating": "4.9/5 (451 ratings)",
        "serial": "54651598453",
        "supplier": "Meatlovers"
    }
]
vegetable = [
    {
        "id": 21,
        "name": "Xiao Bai Chye",
        "weight": "220g",
        "price": 0.80,
        "country": "Singapore",
        "description": "Store at room temperature.",
        "url": "https://s3-ap-southeast-1.amazonaws.com/www8.fairprice.com.sg/fpol/media/images/product/XL/13032618_XL1.jpg",
        "rating": "4.7/5 (251 ratings)",
        "serial": "17611295433",
        "supplier": "Pasar"
    },
    {
        "id": 22,
        "name": "Pearl Brinjal",
        "weight": "300g",
        "price": 1.40,
        "country": "Malaysia",
        "description": "No pesticide. No chemical fertiliser. Advanced Japanese lacto farming technology. Tasty and nutritious.",
        "url": "https://s3-ap-southeast-1.amazonaws.com/www8.fairprice.com.sg/fpol/media/images/product/XL/12605904_XL1.jpg",
        "rating": "4.2/5 (251 ratings)",
        "serial": "27611295433",
        "supplier": "Sakura"
    },
    {
        "id": 23,
        "name": "Prepacked Carrots",
        "weight": "500g",
        "price": 0.90,
        "country": "Australia",
        "description": "The Australian carrots are extremely crunchy and are a rich source of vitamin A and antioxidant agents.",
        "url": "https://s3-ap-southeast-1.amazonaws.com/www8.fairprice.com.sg/fpol/media/images/product/XL/13000321_XL1.jpg",
        "rating": "4.1/5 (251 ratings)",
        "serial": "97611295433",
        "supplier": "Pasar"
    }
]
fruit = [
    {
        "id": 31,
        "name": "Apple Bag - Royal Gala",
        "weight": "1.3kg",
        "price": 5.90,
        "country": "New Zealand",
        "description": "The fruit is two toned with a orange red blush that is commonly striped. The fruit also has sweet yellowish flesh and a crisp texture. The fruit is rich in fibre, antioxidants, vitamin C, A, potassium and iron.",
        "url": "https://s3-ap-southeast-1.amazonaws.com/www8.fairprice.com.sg/fpol/media/images/product/XL/13099853_XL1.jpg",
        "rating": "3.9/5 (251 ratings)",
        "serial": "58734195433",
        "supplier": "Apple Fiesta"
    },
    {
        "id": 32,
        "name": "Hong Kong Papaya",
        "weight": "1.6kg",
        "price": 2.90,
        "country": "Malaysia",
        "description": "The papaya has a red orange and yellow flesh that is soft to the touch. Its flesh is firm, juicy, aromatic and has a buttery sweetness. It has anti-inflammatory properties and contains Vitamin A and C. It can be used as garnish to make dishes attractive, added to fruit salads or blended with peanut butter for a tasty spread.",
        "url": "https://s3-ap-southeast-1.amazonaws.com/www8.fairprice.com.sg/fpol/media/images/product/XL/13004610_XL1.jpg",
        "rating": "4.4/5 (251 ratings)",
        "serial": "58734195433",
        "supplier": "Pasar"
    },
    {
        "id": 33,
        "name": "Delite Australian Mandarins",
        "weight": "600g",
        "price": 4.60,
        "country": "Australia",
        "description": "he fruit's exterior has a beautiful, bright and bumpy rind. Their sparkling orange interior is exceptionally sweet.",
        "url": "https://s3-ap-southeast-1.amazonaws.com/www8.fairprice.com.sg/fpol/media/images/product/XL/13052577_XL1.jpg",
        "rating": "4.3/5 (251 ratings)",
        "serial": "58734195433",
        "supplier": "Sunkist"
    },
    {
        "id": 34,
        "name": "Rock Melon",
        "weight": "1.5kg",
        "price": 8.50,
        "country": "Australia",
        "description": "Sweet but not over-sweet, a ripe cantaloupe has a beautiful floral aroma. The firm flesh has plenty of juice. This popular melon is perfect any time of day, and pairs deliciously with savory meats like prosciutto.",
        "url": "https://s3-ap-southeast-1.amazonaws.com/media.redmart.com/newmedia/1600x/i/m/Sweet_Melon_a_1450227666847.JPG",
        "rating": "4.8/5 (251 ratings)",
        "serial": "12734195433",
        "supplier": "Redmart"
    }
]
dairy = []
carb = []
drink = []


catalogue = {
    "meat": meat,
    "vegetable": vegetable,
    "dairy": dairy,
    "drink": drink,
    "fruit": fruit,
    "carb": carb
}

all_item = []
for items in catalogue:
    all_item += catalogue[items]
mapping = {}
for item in all_item:
    mapping[item["id"]] = item
