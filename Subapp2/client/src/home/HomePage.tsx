import Image from 'react-bootstrap/Image';

export default function HomePage() {
  return (
    <div className="text-center">
      <h1 className="display-4">Monster Munch</h1>
      <p className="lead">Munch your way to an A!</p>
      {/* Static homepage artwork is served with the frontend. */}
      <Image
        src="/images/burger-monster.png"
        alt="Burger monster"
        fluid
        style={{ maxWidth: '300px' }}
      />
    </div>
  );
}