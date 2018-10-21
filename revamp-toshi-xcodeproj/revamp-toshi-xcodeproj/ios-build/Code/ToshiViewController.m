//
//  ToshiViewController.m
//  Unity-iPhone
//
//  Created by Yang Ang on 11/10/18.
//

#import "ToshiViewController.h"
#import <AVFoundation/AVFoundation.h>


@interface ToshiViewController () {
    AVAudioPlayer *_audioPlayer;
}
@end

@implementation ToshiViewController

- (void)viewDidLoad {
    [super viewDidLoad];
    NSString *path = [NSString stringWithFormat:@"%@/toshi-audio.mp3", [[NSBundle mainBundle] resourcePath]];
    NSURL *soundUrl = [NSURL fileURLWithPath:path];
    
    // Create audio player object and initialize with URL to sound
    _audioPlayer = [[AVAudioPlayer alloc] initWithContentsOfURL:soundUrl error:nil];

    [_audioPlayer play];
    // Do any additional setup after loading the view.
}

- (void)didReceiveMemoryWarning {
    [super didReceiveMemoryWarning];
    // Dispose of any resources that can be recreated.
}

- (IBAction)closeView:(id)sender {
    [self dismissViewControllerAnimated:YES completion:nil];
}
@end
