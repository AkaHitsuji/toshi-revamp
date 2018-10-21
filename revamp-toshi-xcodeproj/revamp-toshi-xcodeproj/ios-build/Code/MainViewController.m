//
//  MainViewController.m
//
//  Created by Yang Ang on 7/10/18.

#import "MainViewController.h"
#import "ARViewController.h"
#import "MainViewCollectionViewCell.h"
#import "ListTableViewCell.h"

@interface MainViewController () {
    NSArray *listImages;
    NSArray *listTitles;
    NSArray *consolidatedListTitle;
    NSArray *consolidatedListQty;
    NSArray *consolidatedListPrice;
    __weak IBOutlet UICollectionView *collectionView;
    __weak IBOutlet UITableView *tableView;
}
@end

@implementation MainViewController

- (id)initWithNibName:(NSString *)nibNameOrNil bundle:(NSBundle *)nibBundleOrNil
{
    self = [super initWithNibName:nibNameOrNil bundle:nibBundleOrNil];
    if (self) {
        // Custom initialization
    }
    return self;
}

- (void)viewDidLoad
{
    [super viewDidLoad];
    
    [self getData];
    
    listTitles = [NSArray arrayWithObjects:@"Personal List", @"Vegetable List", @"Chicken Rice", @"Spaghetti Bolognese", @"Essentials List", @"Condiments List", @"Pizza", @"Eggs Benedict", nil];
    listImages = [NSArray arrayWithObjects:@"https://food.fnr.sndimg.com/content/dam/images/food/fullset/2011/5/9/0/FNM_060111-Insert-008-u_s4x3.jpg.rend.hgtvcom.616.462.suffix/1371597483472.jpeg", @"https://www.bbcgoodfood.com/sites/default/files/recipe-collections/collection-image/2013/05/frying-pan-pizza-easy-recipe-collection.jpg", @"https://media.mnn.com/assets/images/2016/05/Fresh-fruit-pretty.jpg.653x0_q80_crop-smart.jpg", @"https://www.moms-mexican-recipes.com/wp-content/uploads/2017/02/Mexican-Seafood-Soup-Ingredients.jpg", @"https://www.bbcgoodfood.com/sites/default/files/recipe-collections/collection-image/2013/05/classic-italian-lasagne-2.jpg", @"https://images-gmi-pmc.edge-generalmills.com/17dffca0-7f96-4d69-8ce0-fb88830b543e.jpg", @"https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTIjfXTZRXdSvf5nycPPh4jQ89B4rxiyGY7TPjZPEfRJeaxfhXytg", @"https://www.aspicyperspective.com/wp-content/uploads/2017/02/best-italian-pasta-salad-13.jpg", nil];
    
    collectionView.delegate = self;
    collectionView.dataSource = self;
    [collectionView reloadData];
    
    // My project use navigation controller just for transition animation right to left, thats why i hide it here on first view.
    [self.navigationController setNavigationBarHidden:YES];
}

- (void)didReceiveMemoryWarning
{
    [super didReceiveMemoryWarning];
    // Dispose of any resources that can be recreated.
}

// Populating collection view
- (NSInteger)collectionView:(UICollectionView *)collectionView numberOfItemsInSection:(NSInteger)section {
    return listImages.count;
}

- (UICollectionViewCell *)collectionView:(UICollectionView *)collectionView cellForItemAtIndexPath:(NSIndexPath *)indexPath{
    
    static NSString *identifier = @"CollectionViewCell";
    MainViewCollectionViewCell *cell = [collectionView dequeueReusableCellWithReuseIdentifier:identifier forIndexPath:indexPath];
    
    NSLog(@"This is the data being sent: %@", [listImages objectAtIndex:indexPath.row]);
   
    UIImageView *collectionImageView = (UIImageView *)cell.image;
//    collectionImageView.image = [UIImage imageNamed:[groceryList objectAtIndex:indexPath.row]];
    
    NSString *ImageURL = [listImages objectAtIndex:indexPath.row];
    NSData *imageData = [NSData dataWithContentsOfURL:[NSURL URLWithString:ImageURL]];
    collectionImageView.image = [UIImage imageWithData:imageData];
    
    // Add rounded corners
    collectionImageView.layer.cornerRadius = 30.0;
    collectionImageView.layer.masksToBounds = YES;
    
    cell.label.text = [listTitles objectAtIndex:indexPath.row];
    
    return cell;
}

// Load list table view when click on collectionViewCell
- (void)collectionView:(UICollectionView *)collectionView didSelectItemAtIndexPath:(NSIndexPath *)indexPath {
    
    [self performSegueWithIdentifier:@"listSegue" sender:self];
    }

- (void)prepareForSegue:(UIStoryboardSegue *)segue sender:(id)sender
{
    // Make sure your segue name in storyboard is the same as this line
    if ([[segue identifier] isEqualToString:@"listSegue"])
    {
        //if you need to pass data to the next controller do it here
    }
}

// Table View methods

- (nonnull UITableViewCell *)tableView:(nonnull UITableView *)tableView cellForRowAtIndexPath:(nonnull NSIndexPath *)indexPath {
    static NSString *TableIdentifier = @"tableItem";
    
    ListTableViewCell *cell = [tableView dequeueReusableCellWithIdentifier:TableIdentifier];
    
    if (cell == nil) {
        cell = [[ListTableViewCell alloc] initWithStyle:UITableViewCellStyleDefault reuseIdentifier:TableIdentifier];
    }
    
    NSLog(@"THis is the data passed: %@, %@, %@",[consolidatedListPrice objectAtIndex:indexPath.row],[NSString stringWithFormat:@"$%@", [consolidatedListPrice objectAtIndex:indexPath.row]],[NSString stringWithFormat:@"Qty: %@", [consolidatedListQty objectAtIndex:indexPath.row]]);
    cell.itemName.text = [consolidatedListTitle objectAtIndex:indexPath.row];
    cell.price.text = [NSString stringWithFormat:@"$%@", [consolidatedListPrice objectAtIndex:indexPath.row]];
    cell.qty.text = [NSString stringWithFormat:@"Qty: %@", [consolidatedListQty objectAtIndex:indexPath.row]];
    cell.backgroundColor = [UIColor whiteColor];
    
    return cell;
}

- (NSInteger)tableView:(nonnull UITableView *)tableView numberOfRowsInSection:(NSInteger)section {
    return [consolidatedListTitle count];
}

-(void)getData {
    NSError *error;
    NSString *url_string = [NSString stringWithFormat: @"http://revamp.ap-southeast-1.elasticbeanstalk.com/user/toshi/shopping"];
    NSData *data = [NSData dataWithContentsOfURL: [NSURL URLWithString:url_string]];
    NSDictionary *json = [NSJSONSerialization JSONObjectWithData:data options:kNilOptions error:&error];
    NSDictionary *itemData = [json valueForKey:@"items"];
//    NSLog(@"json: %@", itemData);
    
    consolidatedListTitle = [itemData valueForKey:@"name"];
    NSLog(@"titles: %@",consolidatedListTitle);
    consolidatedListQty = [itemData valueForKey:@"qty"];
    NSLog(@"qty: %@",consolidatedListQty);
    consolidatedListPrice = [itemData valueForKey:@"price"];
    NSLog(@"price: %@",consolidatedListPrice);

}

// method to open Unity
-(void)touchToLoad:(id)sender
{
    UIStoryboard *storyBoard;
    storyBoard                      = [UIStoryboard storyboardWithName:@"Main" bundle:nil];
    ARViewController *mainVC        = [storyBoard instantiateViewControllerWithIdentifier:@"idARViewController"];
    [self.navigationController pushViewController:mainVC animated:YES];
}

@end
